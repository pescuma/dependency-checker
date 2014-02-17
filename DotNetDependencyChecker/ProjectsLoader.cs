using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker
{
	public class ProjectsLoader
	{
		private readonly Config config;
		private readonly List<RuleMatch> warnings;
		private Func<Dependable, bool> ignore;
		private List<Dependable> projs;
		private List<Dependency> dependencies;
		private List<ProcessingProject> processing;

		public ProjectsLoader(Config config, List<RuleMatch> warnings)
		{
			this.config = config;
			this.warnings = warnings;
		}

		public DependencyGraph LoadGraph()
		{
			projs = new List<Dependable>();
			dependencies = new List<Dependency>();
			ignore = p => config.Ignores.Any(i => i.Matches(p));

			Console.WriteLine("Reading cs projs...");

			var csprojFiles = config.Inputs.SelectMany(folder => Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories))
				.Select(Path.GetFullPath)
				.Distinct();

			processing = csprojFiles.Select(csprojFile =>
			{
				var csproj = new CsprojReader(csprojFile);
				var project = new Project(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, csproj.Filename);
				var ignored = config.Ignores.Any(i => i.Matches(project));

				return new ProcessingProject(csproj, project, ignored);
			})
				.ToList();

			Console.WriteLine("Creating dependency graph...");

			CreateInitialProjects();

			CreateProjectReferences();

			CreateDLLReferences();

			projs.Sort(DependableUtils.NaturalOrdering);
			dependencies.Sort(Dependency.NaturalOrdering);

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
					.Append(((Project) p).GetCsprojOrFullID()));

				throw new ConfigException(msg.ToString());
			}

			projs.Add(newProj);
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

					var refs = FindProjects(proj, dep, p => p.CsprojPath == reference.Include, reference.Include);

					if (refs == null)
					{
						var rp = TryReadCsproj(reference.Include);

						if (rp != null && ignore(rp))
							refs = new List<Project>();

						else if (rp != null)
						{
							AddProject(rp);
							refs = rp.AsList();
						}
					}

					if (refs == null)
						refs = SecondaryFindProjects(proj, dep, reference.Include, //
							p => p.Name == reference.Name && p.Guid == reference.ProjectGuid,
							string.Format("named {0} with GUID {1}", reference.Name, reference.ProjectGuid));

					if (refs == null)
						refs = SecondaryFindProjects(proj, dep, reference.Include, //
							p => p.Name == reference.Name, string.Format("named {0}", reference.Name));

					if (refs == null)
						refs = CreateFakeProject(proj, dep, reference)
							.AsList();

					if (refs == null || !refs.Any())
						continue;

					refs.ForEach(rf => rf.Paths.Add(reference.Include));

					refs.ForEach(rf => dependencies.Add(dep.WithTarget(rf)));
				}
			}
		}

		private static Project TryReadCsproj(string filename)
		{
			try
			{
				var reader = new CsprojReader(filename);
				return new Project(reader.Name, reader.AssemblyName, reader.ProjectGuid, filename);
			}
			catch (IOException)
			{
				return null;
			}
		}

		private List<Project> SecondaryFindProjects(Project proj, Dependency dep, string filename, Func<Project, bool> matches, string refName)
		{
			var result = FindProjects(proj, dep, matches, refName);

			if (result != null && result.Any())
			{
				var message = new OutputMessage();

				message.Append("The project ")
					.Append(proj, OutputMessage.Info.NameAndPath)
					.Append(" references the project ")
					.Append(filename)
					.Append(" but it could not be loaded. Using project")
					.Append(result.Count > 1 ? "s" : "")
					.Append(" ")
					.Append(refName)
					.Append(" instead:");

				result.ForEach(p => message.Append("\n  - ")
					.Append(p, OutputMessage.Info.CsprojOrFullID));

				var allProjs = result.Concat(proj.AsList())
					.ToList();
				warnings.Add(new RuleMatch(false, Severity.Warn, message, null, allProjs, allProjs.Select(dep.WithTarget)));
			}

			return result;
		}

		private Project CreateFakeProject(Project proj, Dependency dep, CsprojReader.ProjectReference reference)
		{
			var result = new Project(reference.Name, reference.Name, reference.ProjectGuid, reference.Include);

			if (ignore(result))
				return null;

			var msg = new OutputMessage().Append("The project ")
				.Append(proj, OutputMessage.Info.NameAndPath)
				.Append(" references the project ")
				.Append(reference.Include)
				.Append(" but it could not be loaded. Guessing assembly name to be the same as project name.");

			warnings.Add(new RuleMatch(false, Severity.Warn, msg, null, dep.WithTarget(result)));

			AddProject(result);

			return result;
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

					var referenceProjs = FindProjects(proj, dep, p => name.Equals(p.AssemblyName, StringComparison.CurrentCultureIgnoreCase),
						"with assembly name " + name);

					if (referenceProjs == null)
					{
						var refProj = new Project(name, name, null, null);
						if (ignore(refProj))
							continue;

						AddProject(refProj);

						referenceProjs = refProj.AsList();
					}

					if (!referenceProjs.Any())
						continue;

					if (reference.HintPath != null)
						referenceProjs.ForEach(rf => rf.Paths.Add(reference.HintPath));

					referenceProjs.ForEach(rf => dependencies.Add(dep.WithTarget(rf)));
				}
			}
		}

		private List<Project> FindProjects(Project proj, Dependency dep, Func<Project, bool> matches, string refName)
		{
			var candidates = processing.Where(p => !p.Ignored && matches(p.Project))
				.Select(i => i.Project)
				.ToList();

			if (!candidates.Any())
				// Search new projects too
				candidates = projs.OfType<Project>()
					.Where(matches)
					.ToList();

			if (candidates.Count == 1)
				return candidates;

			if (candidates.Count > 1)
			{
				warnings.Add(CreateMultipleReferencesWarning(proj, dep, refName, candidates));
				return candidates;
			}

			if (processing.Any(p => p.Ignored && matches(p.Project)))
				// The project exists but was ignored
				return new List<Project>();

			return null;
		}

		private RuleMatch CreateMultipleReferencesWarning(Project proj, Dependency dep, string refName, List<Project> candidates)
		{
			var message = new OutputMessage().Append("The project ")
				.Append(proj, OutputMessage.Info.NameAndPath)
				.Append(" references the project ")
				.Append(refName)
				.Append(", but there are ")
				.Append(candidates.Count)
				.Append(" projects that match:");

			candidates.ForEach(c => message.Append("\n  - ")
				.Append(c, OutputMessage.Info.CsprojOrFullID));
			message.Append("\nMultiple dependencies will be created.");

			return new RuleMatch(false, Severity.Warn, message, null, proj.AsList()
				.Concat(candidates), dep.AsList());
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
