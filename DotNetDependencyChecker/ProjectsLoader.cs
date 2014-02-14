using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker
{
	public class ProjectsLoader
	{
		private readonly Config config;
		private readonly List<RuleMatch> warnings;
		private Func<Project, bool> ignore;
		private HashSet<Project> projs;
		private List<Dependency> dependencies;
		private List<ProcessingProject> processing;

		public ProjectsLoader(Config config, List<RuleMatch> warnings)
		{
			this.config = config;
			this.warnings = warnings;
		}

		public DependencyGraph LoadGraph()
		{
			projs = new HashSet<Project>();
			dependencies = new List<Dependency>();
			ignore = p => config.Ignores.Any(i => i.Matches(p));

			Console.WriteLine("Reading cs projs...");

			var csprojFiles = config.Inputs.SelectMany(folder => Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories))
				.Select(Path.GetFullPath)
				.Distinct();

			processing = csprojFiles.Select(csprojFile =>
			{
				var csproj = new CsprojReader(csprojFile);
				var project = new Project(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, csproj.Filename, true);
				var ignored = config.Ignores.Any(i => i.Matches(project));

				return new ProcessingProject(csproj, project, ignored);
			})
				.ToList();

			Console.WriteLine("Creating dependency graph...");

			processing.Where(i => !i.Ignored)
				.Select(i => i.Project)
				.ForEach(p => projs.Add(p));

			CreateProjectReferences();

			CreateDLLReferences();

			var graph = new DependencyGraph();
			projs.ForEach(p => graph.AddVertex(p));
			dependencies.ForEach(d => graph.AddEdge(d));
			return graph;
		}

		private class ProcessingProject
		{
			public readonly CsprojReader Csproj;
			public readonly Project Project;
			public readonly bool Ignored;

			public ProcessingProject(CsprojReader csproj, Project project, bool ignored)
			{
				Csproj = csproj;
				Project = project;
				Ignored = ignored;
			}
		}

		private void CreateProjectReferences()
		{
			foreach (var item in processing.Where(i => !i.Ignored))
			{
				var csproj = item.Csproj;
				var proj = item.Project;

				foreach (var csref in csproj.ProjectReferences)
				{
					var reference = csref;

					// Dummy reference for logs in case of errors
					var dep = new Dependency(proj, null, Dependency.Types.ProjectReference, new Location(csproj.Filename, reference.LineNumber));

					var referenceProj = FindOrCreateProject(proj, dep, //
						p => p.Name == reference.Name && p.Guid == reference.ProjectGuid,
						string.Format("named {0} with GUI {1}", reference.Name, reference.ProjectGuid), //
						() => TryReadExternalProject(reference, dep));

					if (referenceProj == null)
						continue;

					proj.Paths.Add(reference.Include);

					dependencies.Add(dep.WithTarget(referenceProj));
				}
			}
		}

		private void CreateDLLReferences()
		{
			foreach (var item in processing.Where(i => !i.Ignored))
			{
				var csproj = item.Csproj;
				var proj = item.Project;

				foreach (var reference in csproj.References)
				{
					var name = reference.Include.Name;

					// Dummy reference for logs in case of errors
					var dep = new Dependency(proj, null, Dependency.Types.ProjectReference, new Location(csproj.Filename, reference.LineNumber));

					var referenceProj = FindOrCreateProject(proj, dep, //
						p => p.AssemblyName == name, "with assembly name " + name, //
						() => new Project(name, name, null, null, false));

					if (referenceProj == null)
						continue;

					if (reference.HintPath != null)
						referenceProj.Paths.Add(reference.HintPath);

					dependencies.Add(dep.WithTarget(referenceProj));
				}
			}
		}

		private Project FindOrCreateProject(Project proj, Dependency dep, Func<Project, bool> matches, string refName,
			Func<Project> createNewProject)
		{
			var candidates = processing.Where(p => !p.Ignored && matches(p.Project))
				.Select(i => i.Project)
				.ToList();

			if (candidates.Count == 1)
				return candidates.First();

			if (candidates.Count > 1)
			{
				warnings.Add(CreateMultipleReferencesWarning(proj, dep, refName, candidates));
				return null;
			}

			// candidates.Count < 1

			if (processing.Any(p => p.Ignored && matches(p.Project)))
				// The project exists but was ignored
				return null;

			var result = createNewProject();
			if (ignore(result))
				return null;

			projs.Add(result);

			return result;
		}

		private RuleMatch CreateMultipleReferencesWarning(Project proj, Dependency dep, string refName, List<Project> candidates)
		{
			var msg = new StringBuilder();

			msg.Append(string.Format("The project {0} references the project {1}, but there are {2} projects that match:", proj.Name, refName,
				candidates.Count));

			candidates.ForEach(c =>
			{
				msg.Append("\n  - ");
				if (c.CsprojPath != null)
				{
					msg.Append(c.CsprojPath);
				}
				else
				{
					msg.Append(c.Name);

					if (c.AssemblyName != c.Name)
						msg.Append(", Assembly name: ")
							.Append(c.AssemblyName);

					if (c.Guid != null)
						msg.Append(", GUID: ")
							.Append(c.Guid);
				}
			});

			msg.Append("\nThis dependecy will be ignored.");

			return new RuleMatch(false, Severity.Error, msg.ToString(), null, new List<Project> { proj }.Concat(candidates),
				new List<Dependency> { dep });
		}

		private Project TryReadExternalProject(CsprojReader.ProjectReference reference, Dependency dep)
		{
			string filename = reference.Include;
			try
			{
				var csproj = new CsprojReader(filename);
				return new Project(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, filename, false);
			}
			catch (IOException)
			{
				var result = new Project(reference.Name, reference.Name, reference.ProjectGuid, null, false, filename);

				var msg = string.Format("Could not load project {0}: guessing assembly name to be the same as project name", filename);
				warnings.Add(new RuleMatch(false, Severity.Warn, msg, null, dep.WithTarget(result)));

				return result;
			}
		}
	}
}
