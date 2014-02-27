using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker
{
	public class ProjectsLoader
	{
		private readonly Config config;
		private readonly List<OutputEntry> warnings;
		private Func<Dependable, bool> ignore;
		private List<Assembly> assemblies;
		private List<Dependency> dependencies;
		private List<ProcessingProject> processing;

		public ProjectsLoader(Config config, List<OutputEntry> warnings)
		{
			this.config = config;
			this.warnings = warnings;
		}

		public DependencyGraph LoadGraph()
		{
			assemblies = new List<Assembly>();
			dependencies = new List<Dependency>();
			ignore = p => config.Ignores.Any(i => i.Matches(p));

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

			CreateInitialProjects();

			CreateProjectReferences();

			CreateDLLReferences();

			assemblies.Sort(DependableUtils.NaturalOrdering);
			dependencies.Sort(Dependency.NaturalOrdering);

			var graph = new DependencyGraph();
			assemblies.ForEach(p => graph.AddVertex(p));
			dependencies.ForEach(d => graph.AddEdge(d));
			return graph;
		}

		private void CreateInitialProjects()
		{
			processing.Where(i => !i.Ignored)
				.Select(i => i.Project)
				.ForEach(AddAssembly);
		}

		private void AddAssembly(Assembly newAssembly)
		{
			var sameAssemblies = assemblies.Where(p => AreTheSame(p, newAssembly))
				.ToList();

			if (sameAssemblies.Any())
			{
				sameAssemblies.Add(newAssembly);

				var msg = new StringBuilder();
				msg.Append("There are ")
					.Append(sameAssemblies.Count)
					.Append(" projects that are the same:");
				sameAssemblies.ForEach(p => msg.Append("\n  - ")
					.Append(((Project) p).CsprojPath));

				throw new ConfigException(msg.ToString());
			}

			assemblies.Add(newAssembly);
		}

		private bool AreTheSame(Assembly a1, Assembly a2)
		{
			if (a1.Equals(a2))
				return true;

			// Handle Proj vs Assembly
			if (!(a1 is Project) || !(a2 is Project))
				return a1.AssemblyName.Equals(a2.AssemblyName);

			return false;
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

					var refs = FindAssembly(proj, dep,
						p => string.Equals(((Project) p).CsprojPath, reference.Include, StringComparison.CurrentCultureIgnoreCase), reference.Include);

					if (refs == null)
					{
						var rp = TryReadCsproj(reference.Include);

						if (rp != null && ignore(rp))
							refs = new List<Assembly>();

						else if (rp != null)
						{
							AddAssembly(rp);
							refs = rp.AsList<Assembly>();
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
							.AsList<Assembly>();

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

		private List<Assembly> SecondaryFindProjects(Project proj, Dependency dep, string filename, Func<Project, bool> matches, string refName)
		{
			var result = FindAssembly(proj, dep, p => matches((Project) p), refName);

			if (result != null && result.Any())
			{
				var message = new OutputMessage().Append("The project ")
					.Append(proj, OutputMessage.ProjInfo.Name)
					.Append(" references the project ")
					.Append(filename)
					.Append(" but it could not be loaded. Using project")
					.Append(result.Count > 1 ? "s" : "")
					.Append(" ")
					.Append(refName)
					.Append(" instead:");

				if (result.Count == 1)
					result.ForEach(p => message.Append(result.First(), OutputMessage.ProjInfo.Csproj));
				else
					result.ForEach(p => message.Append("\n  - ")
						.Append(p, OutputMessage.ProjInfo.Csproj));

				warnings.Add(new LoadingOutputWarning("Only similar project found", message, result.Select(dep.WithTarget)
					.ToArray()));
			}

			return result;
		}

		private Project CreateFakeProject(Project proj, Dependency dep, CsprojReader.ProjectReference reference)
		{
			var result = new Project(reference.Name, reference.Name, reference.ProjectGuid, reference.Include);

			if (ignore(result))
				return null;

			var msg = new OutputMessage().Append("The project ")
				.Append(proj, OutputMessage.ProjInfo.Name)
				.Append(" references the project ")
				.Append(reference.Include)
				.Append(" but it could not be loaded. Guessing assembly name to be the same as project name.");

			warnings.Add(new LoadingOutputWarning("Project not found", msg, dep.WithTarget(result)));

			AddAssembly(result);

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
					var dep = new Dependency(proj, null, Dependency.Types.DllReference, new Location(csproj.Filename, reference.LineNumber));

					var referenceProjs = FindAssembly(proj, dep, p => name.Equals(p.AssemblyName, StringComparison.CurrentCultureIgnoreCase),
						"with assembly name " + name);

					if (referenceProjs == null)
					{
						var refProj = new Assembly(name);
						if (ignore(refProj))
							continue;

						AddAssembly(refProj);

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

		private List<Assembly> FindAssembly(Project proj, Dependency dep, Func<Assembly, bool> matches, string refName)
		{
			var candidates = processing.Where(p => !p.Ignored && matches(p.Project))
				.Select(i => i.Project)
				.Cast<Assembly>()
				.ToList();

			if (!candidates.Any())
				// Search new projects too
				candidates = assemblies.Where(matches)
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
				return new List<Assembly>();

			return null;
		}

		private OutputEntry CreateMultipleReferencesWarning(Project proj, Dependency dep, string refName, List<Assembly> candidates)
		{
			var message = new OutputMessage().Append("The project ")
				.Append(proj, OutputMessage.ProjInfo.NameAndCsproj)
				.Append(" references the project ")
				.Append(refName)
				.Append(", but there are ")
				.Append(candidates.Count)
				.Append(" projects that match:");

			candidates.ForEach(c => message.Append("\n  - ")
				.Append(c, OutputMessage.ProjInfo.Csproj));
			message.Append("\nMultiple dependencies will be created.");

			return new LoadingOutputWarning("Multiple projects found", message, candidates.Select(dep.WithTarget)
				.ToArray());
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
