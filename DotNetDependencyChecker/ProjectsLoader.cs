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
		private List<Project> projs;
		private List<Dependency> dependencies;
		private List<ProcessingProject> processing;

		public ProjectsLoader(Config config, List<RuleMatch> warnings)
		{
			this.config = config;
			this.warnings = warnings;
		}

		public DependencyGraph LoadGraph()
		{
			projs = new List<Project>();
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

			CreateInitialProjects();

			CreateProjectReferences();

			CreateDLLReferences();

			var graph = new DependencyGraph();
			projs.ForEach(p => graph.AddVertex(p));
			dependencies.ForEach(d => graph.AddEdge(d));
			return graph;
		}

		private void CreateInitialProjects()
		{
			processing.Where(i => !i.Ignored)
				.Select(i => i.Project)
				.ForEach(AddProject);
		}

		private void AddProject(Project newProj)
		{
			var sameProjs = projs.Where(p => p.Equals(newProj))
				.ToList();

			if (sameProjs.Any())
			{
				sameProjs.Add(newProj);

				var msg = new StringBuilder();
				msg.Append("There are ")
					.Append(sameProjs.Count)
					.Append(" projects that have the same:");
				sameProjs.ForEach(p => msg.Append("\n  - ")
					.Append(ToGui(p)));

				throw new ConfigException(msg.ToString());
			}

			projs.Add(newProj);
		}

		private string ToGui(Project proj)
		{
			var msg = new StringBuilder();

			if (proj.CsprojPath != null)
			{
				msg.Append(proj.CsprojPath);
			}
			else
			{
				msg.Append(proj.Name);

				if (proj.AssemblyName != proj.Name)
					msg.Append(", Assembly name: ")
						.Append(proj.AssemblyName);

				if (proj.Guid != null)
					msg.Append(", GUID: ")
						.Append(proj.Guid);
			}

			return msg.ToString();
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

					var referenceProjs = FindOrCreateProjects(proj, dep, //
						p => p.CsprojPath == reference.Include, reference.Include, //
						() => TryReadExternalProject(reference, dep));

					if (referenceProjs == null)
						continue;

					referenceProjs.ForEach(rf => rf.Paths.Add(reference.Include));

					referenceProjs.ForEach(rf => dependencies.Add(dep.WithTarget(rf)));
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

					var referenceProjs = FindOrCreateProjects(proj, dep, //
						p => p.AssemblyName == name, "with assembly name " + name, //
						() => new Project(name, name, null, null, false));

					if (referenceProjs == null)
						continue;

					if (reference.HintPath != null)
						referenceProjs.ForEach(rf => rf.Paths.Add(reference.HintPath));

					referenceProjs.ForEach(rf => dependencies.Add(dep.WithTarget(rf)));
				}
			}
		}

		private List<Project> FindOrCreateProjects(Project proj, Dependency dep, Func<Project, bool> matches, string refName,
			Func<Project> createNewProject)
		{
			var candidates = processing.Where(p => !p.Ignored && matches(p.Project))
				.Select(i => i.Project)
				.ToList();

			if (!candidates.Any())
				// Search new projects too
				candidates = projs.Where(matches)
					.ToList();

			if (candidates.Count == 1)
				return candidates;

			if (candidates.Count > 1)
			{
				warnings.Add(CreateMultipleReferencesWarning(proj, dep, refName, candidates));
				return candidates;
			}

			// candidates.Count < 1

			if (processing.Any(p => p.Ignored && matches(p.Project)))
				// The project exists but was ignored
				return null;

			var result = createNewProject();
			if (ignore(result))
				return null;

			AddProject(result);

			return result.AsList();
		}

		private RuleMatch CreateMultipleReferencesWarning(Project proj, Dependency dep, string refName, List<Project> candidates)
		{
			var msg = new StringBuilder();

			msg.Append(string.Format("The project {0} references the project {1}, but there are {2} projects that match:", proj.Name, refName,
				candidates.Count));
			candidates.ForEach(c => msg.Append("\n  - ")
				.Append(ToGui(c)));
			msg.Append("\nMultiple dependencies will be created.");

			return new RuleMatch(false, Severity.Warn, msg.ToString(), null, proj.AsList()
				.Concat(candidates), dep.AsList());
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
				var result = new Project(reference.Name, reference.Name, reference.ProjectGuid, filename, false);

				var msg = string.Format("Could not load project {0}: guessing assembly name to be the same as project name", filename);
				warnings.Add(new RuleMatch(false, Severity.Warn, msg, null, dep.WithTarget(result)));

				return result;
			}
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

			public override string ToString()
			{
				return Csproj.Filename + (Ignored ? " [ignored]" : "");
			}
		}
	}
}
