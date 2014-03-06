using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;
using org.pescuma.dotnetdependencychecker.rules;
using org.pescuma.dotnetdependencychecker.utils;

namespace org.pescuma.dotnetdependencychecker.input
{
	public class DependencyGraphBuilder
	{
		private readonly Dictionary<TempProject, TempProject> projs = new Dictionary<TempProject, TempProject>();
		private readonly List<TempReference> refs = new List<TempReference>();

		private readonly Config config;
		private readonly List<OutputEntry> warnings;
		private readonly List<Assembly> assemblies = new List<Assembly>();
		private readonly List<Dependency> dependencies = new List<Dependency>();

		public DependencyGraphBuilder(Config config, List<OutputEntry> warnings)
		{
			this.config = config;
			this.warnings = warnings;
		}

		public object AddProject(string name, string assemblyName, Guid projectGuid, string filename)
		{
			var proj = new TempProject(name, assemblyName, projectGuid, filename);

			TempProject result;
			if (!projs.TryGetValue(proj, out result))
			{
				projs.Add(proj, proj);
				result = proj;
			}

			return result;
		}

		public void AddProjectReference(object proj, string referenceName, string referenceAssemblyName, Guid? referenceGuid,
			string referenceFilename, Location referenceLocation)
		{
			refs.Add(new TempReference((TempProject) proj, Dependency.Types.ProjectReference, referenceName, referenceAssemblyName, referenceGuid,
				referenceFilename, referenceLocation));
		}

		public void AddDllReference(object proj, string referenceName, string referenceAssemblyName, Guid? referenceGuid, string referenceFilename,
			Location referenceLocation)
		{
			refs.Add(new TempReference((TempProject) proj, Dependency.Types.DllReference, referenceName, referenceAssemblyName, referenceGuid,
				referenceFilename, referenceLocation));
		}

		public DependencyGraph Build()
		{
			projs.Keys.ForEach(p =>
			{
				p.Project = new Project(p.Name, p.AssemblyName, p.ProjectGuid, p.Filename);
				p.Ignored = Ignore(p.Project);
			});

			CreateInitialProjects();

			CreateProjectReferences();

			CreateDLLReferences();

			assemblies.Sort(Assembly.NaturalOrdering);
			dependencies.Sort(Dependency.NaturalOrdering);

			var graph = new DependencyGraph();
			assemblies.ForEach(p => graph.AddVertex(p));
			dependencies.ForEach(d => graph.AddEdge(d));
			return graph;
		}

		private bool Ignore(Assembly assembly)
		{
			return config.Ignores.Any(i => i.Matches(assembly));
		}

		private void CreateInitialProjects()
		{
			projs.Keys.Where(i => !i.Ignored)
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
			foreach (var tmp in refs.Where(r => r.Type == Dependency.Types.ProjectReference && !r.Source.Ignored))
			{
				var projRef = tmp;
				var proj = projRef.Source.Project;

				// Dummy reference for logs in case of errors
				var dep = Dependency.WithProject(proj, null, projRef.ReferenceLocation);

				List<Assembly> found = null;

				if (projRef.ReferenceFilename != null)
					found = FindAssemblyByFilename(proj, dep, projRef.ReferenceFilename);

				if (found == null && projRef.ReferenceName != null && projRef.ReferenceGuid != null)
				{
					found = FindAssemblyByNameAndGuid(proj, dep, projRef.ReferenceName, projRef.ReferenceGuid.Value);

					if (projRef.ReferenceFilename != null)
						WarnIfSimilarFound(found, proj, dep, projRef.ReferenceFilename,
							string.Format("named {0} with GUID {1}", projRef.ReferenceName, projRef.ReferenceGuid));
				}

				if (found == null && projRef.ReferenceName != null)
				{
					found = FindAssemblyByName(proj, dep, projRef.ReferenceName);

					if (projRef.ReferenceFilename != null)
						WarnIfSimilarFound(found, proj, dep, projRef.ReferenceFilename, string.Format("named {0}", projRef.ReferenceName));
				}

				if (found == null)
					found = CreateFakeProject(proj, dep, projRef)
						.AsList<Assembly>();

				if (found == null || !found.Any())
					continue;

				if (projRef.ReferenceFilename != null)
					found.ForEach(rf => rf.Paths.Add(projRef.ReferenceFilename));

				found.ForEach(rf => dependencies.Add(dep.WithTarget(rf)));
			}
		}

		private List<Assembly> FindAssemblyByFilename(Project proj, Dependency dep, string filename)
		{
			return FindAssembly(proj, dep, p => p.Paths.Any(path => filename.Equals(path, StringComparison.CurrentCultureIgnoreCase)), filename);
		}

		private List<Assembly> FindAssemblyByNameAndAssemblyName(Project proj, Dependency dep, string name, string assemblyName)
		{
			return FindAssembly(proj, dep,
				p =>
					name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase)
					&& assemblyName.Equals(p.AssemblyName, StringComparison.CurrentCultureIgnoreCase),
				"with name " + name + " and assembly name " + assemblyName);
		}

		private List<Assembly> FindAssemblyByAssemblyName(Project proj, Dependency dep, string assemblyName)
		{
			return FindAssembly(proj, dep, p => assemblyName.Equals(p.AssemblyName, StringComparison.CurrentCultureIgnoreCase),
				"with assembly name " + assemblyName);
		}

		private List<Assembly> FindAssemblyByName(Project proj, Dependency dep, string name)
		{
			return FindAssembly(proj, dep, p => name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase), "with name " + name);
		}

		private List<Assembly> FindAssemblyByNameAndGuid(Project proj, Dependency dep, string name, Guid guid)
		{
			return FindAssembly(proj, dep,
				p => p is Project && name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase) && ((Project) p).Guid == guid,
				"with name " + name + " and GUID " + guid);
		}

		private List<Assembly> FindAssembly(Project proj, Dependency dep, Func<Assembly, bool> matches, string searchDetails)
		{
			var candidates = projs.Keys.Where(p => !p.Ignored && matches(p.Project))
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
				warnings.Add(CreateMultipleReferencesWarning(proj, dep, searchDetails, candidates));
				return candidates;
			}

			if (projs.Keys.Any(p => p.Ignored && matches(p.Project)))
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

		private void WarnIfSimilarFound(List<Assembly> result, Project proj, Dependency dep, string filename, string refName)
		{
			if (result == null || !result.Any())
				return;

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

		private Project CreateFakeProject(Project proj, Dependency dep, TempReference reference)
		{
			var result = new Project(reference.ReferenceName ?? reference.ReferenceAssemblyName,
				reference.ReferenceAssemblyName ?? reference.ReferenceName, reference.ReferenceGuid ?? Guid.NewGuid(), reference.ReferenceFilename);

			if (Ignore(result))
				return null;

			if (reference.ReferenceName == null || reference.ReferenceAssemblyName == null)
			{
				var guessed = reference.ReferenceName == null ? "project name" : "assembly name";
				var used = reference.ReferenceName == null ? "assembly name" : "project name";

				var msg = new OutputMessage().Append("The project ")
					.Append(proj, OutputMessage.ProjInfo.Name)
					.Append(" references the project ")
					.Append(reference.ReferenceFilename)
					.Append(" but it could not be loaded. Guessing the ")
					.Append(guessed)
					.Append(" to be the same as the ")
					.Append(used)
					.Append(".");

				warnings.Add(new LoadingOutputWarning("Project not found", msg, dep.WithTarget(result)));
			}

			AddAssembly(result);

			return result;
		}

		private void CreateDLLReferences()
		{
			foreach (var tmp in refs.Where(r => r.Type == Dependency.Types.DllReference && !r.Source.Ignored))
			{
				var projRef = tmp;
				var proj = projRef.Source.Project;

				// Dummy reference for logs in case of errors
				var dep = Dependency.WithAssembly(proj, null, projRef.ReferenceLocation, projRef.ReferenceFilename);

				List<Assembly> found = null;

				if (projRef.ReferenceFilename != null)
					found = FindAssemblyByFilename(proj, dep, projRef.ReferenceFilename);

				if (projRef.ReferenceName != null && projRef.ReferenceAssemblyName != null)
					found = FindAssemblyByNameAndAssemblyName(proj, dep, projRef.ReferenceName, projRef.ReferenceAssemblyName);

				if (projRef.ReferenceAssemblyName != null)
					found = FindAssemblyByAssemblyName(proj, dep, projRef.ReferenceAssemblyName);

				if (projRef.ReferenceName != null)
					found = FindAssemblyByName(proj, dep, projRef.ReferenceName);

				if (found == null)
				{
					var assembly = new Assembly(projRef.ReferenceAssemblyName ?? projRef.ReferenceName);
					if (!Ignore(assembly))
					{
						AddAssembly(assembly);
						found = assembly.AsList();
					}
				}

				if (found == null || !found.Any())
					continue;

				if (projRef.ReferenceFilename != null)
					found.ForEach(rf => rf.Paths.Add(projRef.ReferenceFilename));

				found.ForEach(rf => dependencies.Add(dep.WithTarget(rf)));
			}
		}

		private class TempProject
		{
			public readonly string Name;
			public readonly string AssemblyName;
			public readonly Guid ProjectGuid;
			public readonly string Filename;

			public Project Project;
			public bool Ignored;

			public TempProject(string name, string assemblyName, Guid projectGuid, string filename)
			{
				Argument.ThrowIfNull(name);
				Argument.ThrowIfNull(assemblyName);
				Argument.ThrowIfNull(filename);

				Name = name;
				AssemblyName = assemblyName;
				ProjectGuid = projectGuid;
				Filename = filename;
			}

			private bool Equals(TempProject other)
			{
				return string.Equals(Name, other.Name) && string.Equals(AssemblyName, other.AssemblyName) && ProjectGuid.Equals(other.ProjectGuid)
				       && string.Equals(Filename, other.Filename);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
					return false;
				if (ReferenceEquals(this, obj))
					return true;
				if (obj.GetType() != GetType())
					return false;
				return Equals((TempProject) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = (Name != null ? Name.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ ProjectGuid.GetHashCode();
					hashCode = (hashCode * 397) ^ (Filename != null ? Filename.GetHashCode() : 0);
					return hashCode;
				}
			}
		}

		private class TempReference
		{
			public readonly TempProject Source;
			public readonly Dependency.Types Type;
			public readonly string ReferenceName;
			public readonly string ReferenceAssemblyName;
			public readonly Guid? ReferenceGuid;
			public readonly string ReferenceFilename;
			public readonly Location ReferenceLocation;

			public TempReference(TempProject source, Dependency.Types type, string referenceName, string referenceAssemblyName, Guid? referenceGuid,
				string referenceFilename, Location referenceLocation)
			{
				Argument.ThrowIfNull(source);
				Argument.ThrowIfAllNull(referenceName, referenceAssemblyName);
				Argument.ThrowIfNull(referenceLocation);

				Source = source;
				Type = type;
				ReferenceName = referenceName;
				ReferenceAssemblyName = referenceAssemblyName;
				ReferenceGuid = referenceGuid;
				ReferenceFilename = referenceFilename;
				ReferenceLocation = referenceLocation;
			}
		}
	}
}
