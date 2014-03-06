using System;
using System.Collections.Generic;
using System.Globalization;
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
		private readonly List<Library> libraries = new List<Library>();
		private readonly List<Dependency> dependencies = new List<Dependency>();

		public DependencyGraphBuilder(Config config, List<OutputEntry> warnings)
		{
			this.config = config;
			this.warnings = warnings;
		}

		public object AddProject(string name, string libraryName, Guid? guid, string filename)
		{
			var proj = new TempProject(name, libraryName, guid, filename);

			TempProject result;
			if (!projs.TryGetValue(proj, out result))
			{
				projs.Add(proj, proj);
				result = proj;
			}

			result.Names.Add(name);
			result.LibraryNames.Add(name);

			return result;
		}

		public void AddProjectReference(object proj, string referenceName, string referenceLibraryName, Guid? referenceGuid,
			string referenceFilename, Location referenceLocation)
		{
			refs.Add(new TempReference((TempProject) proj, Dependency.Types.ProjectReference, referenceName, referenceLibraryName, referenceGuid,
				referenceFilename, referenceLocation));
		}

		public void AddLibraryReference(object proj, string referenceName, string referenceLibraryName, Guid? referenceGuid,
			string referenceFilename, Location referenceLocation)
		{
			refs.Add(new TempReference((TempProject) proj, Dependency.Types.LibraryReference, referenceName, referenceLibraryName, referenceGuid,
				referenceFilename, referenceLocation));
		}

		public DependencyGraph Build()
		{
			projs.Keys.ForEach(p =>
			{
				p.Project = new Project(p.Name, p.LibraryName, p.Guid, p.Filename);
				p.Project.Names.AddRange(p.Names.Where(n => !p.Project.Names.Contains(n))
					.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase));
				p.Project.LibraryNames.AddRange(p.LibraryNames.Where(n => !p.Project.LibraryNames.Contains(n))
					.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase));
				p.Ignored = Ignore(p.Project);
			});

			CreateInitialProjects();

			// Create project references first so we can create the entries as Project if possible
			CreateProjectReferences();

			CreateLibraryReferences();

			libraries.Sort(Library.NaturalOrdering);
			dependencies.Sort(Dependency.NaturalOrdering);

			var graph = new DependencyGraph();
			libraries.ForEach(p => graph.AddVertex(p));
			dependencies.ForEach(d => graph.AddEdge(d));
			return graph;
		}

		private bool Ignore(Library library)
		{
			return config.Ignores.Any(i => i.Matches(library));
		}

		private void CreateInitialProjects()
		{
			projs.Keys.Where(i => !i.Ignored)
				.Select(i => i.Project)
				.ForEach(AddLibrary);
		}

		private void AddLibrary(Library newLibrary)
		{
			var sameLibraries = libraries.Where(p => AreTheSame(p, newLibrary))
				.ToList();

			if (sameLibraries.Any())
			{
				sameLibraries.Add(newLibrary);

				var msg = new StringBuilder();
				msg.Append("There are ")
					.Append(sameLibraries.Count)
					.Append(" projects that are the same:");
				sameLibraries.ForEach(p => msg.Append("\n  - ")
					.Append(((Project) p).ProjectPath));

				throw new ConfigException(msg.ToString());
			}

			libraries.Add(newLibrary);
		}

		private bool AreTheSame(Library a1, Library a2)
		{
			if (a1.Equals(a2))
				return true;

			// Handle Proj vs Library
			if (!(a1 is Project) || !(a2 is Project))
				return a1.LibraryName.Equals(a2.LibraryName);

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

				List<Library> found = null;

				if (projRef.ReferenceFilename != null)
					found = FindLibraryByFilename(proj, dep, projRef.ReferenceFilename);

				if (found == null && projRef.ReferenceName != null && projRef.ReferenceGuid != null)
				{
					found = FindLibraryByNameAndGuid(proj, dep, projRef.ReferenceName, projRef.ReferenceGuid.Value);

					if (projRef.ReferenceFilename != null)
						WarnIfSimilarFound(found, proj, dep, projRef.ReferenceFilename,
							string.Format("named {0} with GUID {1}", projRef.ReferenceName, projRef.ReferenceGuid));
				}

				if (found == null && projRef.ReferenceName != null)
				{
					found = FindLibraryByName(proj, dep, projRef.ReferenceName);

					if (projRef.ReferenceFilename != null)
						WarnIfSimilarFound(found, proj, dep, projRef.ReferenceFilename, string.Format("named {0}", projRef.ReferenceName));
				}

				if (found == null && projRef.ReferenceLibraryName != null)
				{
					found = FindLibraryByLibraryName(proj, dep, projRef.ReferenceLibraryName);

					if (projRef.ReferenceFilename != null)
						WarnIfSimilarFound(found, proj, dep, projRef.ReferenceFilename, string.Format("with library name {0}", projRef.ReferenceLibraryName));
				}

				if (found == null)
					found = CreateFakeProject(proj, dep, projRef)
						.AsList<Library>();

				if (found == null || !found.Any())
					continue;

				foreach (var target in found)
				{
					if (projRef.ReferenceFilename != null)
						target.Paths.Add(projRef.ReferenceFilename);

					if (projRef.ReferenceName != null)
						target.Names.Add(projRef.ReferenceName);

					if (projRef.ReferenceLibraryName != null)
						target.LibraryNames.Add(projRef.ReferenceLibraryName);

					dependencies.Add(dep.WithTarget(target));
				}
			}
		}

		private List<Library> FindLibraryByFilename(Project proj, Dependency dep, string filename)
		{
			return FindLibrary(proj, dep, p => p.Paths.Any(path => filename.Equals(path, StringComparison.CurrentCultureIgnoreCase)), filename);
		}

		private List<Library> FindLibraryByNameAndLibraryName(Project proj, Dependency dep, string name, string libraryName)
		{
			return FindLibrary(proj, dep,
				p =>
					name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase)
					&& libraryName.Equals(p.LibraryName, StringComparison.CurrentCultureIgnoreCase),
				"with name " + name + " and library name " + libraryName);
		}

		private List<Library> FindLibraryByLibraryName(Project proj, Dependency dep, string libraryName)
		{
			return FindLibrary(proj, dep, p => libraryName.Equals(p.LibraryName, StringComparison.CurrentCultureIgnoreCase),
				"with library name " + libraryName);
		}

		private List<Library> FindLibraryByName(Project proj, Dependency dep, string name)
		{
			return FindLibrary(proj, dep, p => name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase), "with name " + name);
		}

		private List<Library> FindLibraryByNameAndGuid(Project proj, Dependency dep, string name, Guid guid)
		{
			return FindLibrary(proj, dep,
				p => p is Project && name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase) && ((Project) p).Guid == guid,
				"with name " + name + " and GUID " + guid);
		}

		private List<Library> FindLibrary(Project proj, Dependency dep, Func<Library, bool> matches, string searchDetails)
		{
			var candidates = projs.Keys.Where(p => !p.Ignored && matches(p.Project))
				.Select(i => i.Project)
				.Cast<Library>()
				.ToList();

			if (!candidates.Any())
				// Search new projects too
				candidates = libraries.Where(matches)
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
				return new List<Library>();

			return null;
		}

		private OutputEntry CreateMultipleReferencesWarning(Project proj, Dependency dep, string refName, List<Library> candidates)
		{
			var message = new OutputMessage().Append("The project ")
				.Append(proj, OutputMessage.ProjInfo.NameAndProjectPath)
				.Append(" references the project ")
				.Append(refName)
				.Append(", but there are ")
				.Append(candidates.Count)
				.Append(" projects that match:");

			candidates.ForEach(c => message.Append("\n  - ")
				.Append(c, OutputMessage.ProjInfo.ProjectPath));
			message.Append("\nMultiple dependencies will be created.");

			return new LoadingOutputWarning("Multiple projects found", message, candidates.Select(dep.WithTarget)
				.ToArray());
		}

		private void WarnIfSimilarFound(List<Library> result, Project proj, Dependency dep, string filename, string refName)
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
				result.ForEach(p => message.Append(result.First(), OutputMessage.ProjInfo.ProjectPath));
			else
				result.ForEach(p => message.Append("\n  - ")
					.Append(p, OutputMessage.ProjInfo.ProjectPath));

			warnings.Add(new LoadingOutputWarning("Only similar project found", message, result.Select(dep.WithTarget)
				.ToArray()));
		}

		private Project CreateFakeProject(Project proj, Dependency dep, TempReference reference)
		{
			var result = new Project(reference.ReferenceName ?? reference.ReferenceLibraryName,
				reference.ReferenceLibraryName ?? reference.ReferenceName, reference.ReferenceGuid ?? Guid.NewGuid(), reference.ReferenceFilename);

			if (Ignore(result))
				return null;

			if (reference.ReferenceName == null || reference.ReferenceLibraryName == null)
			{
				var guessed = reference.ReferenceName == null ? "project name" : "library name";
				var used = reference.ReferenceName == null ? "library name" : "project name";

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

			AddLibrary(result);

			return result;
		}

		private void CreateLibraryReferences()
		{
			foreach (var tmp in refs.Where(r => r.Type == Dependency.Types.LibraryReference && !r.Source.Ignored))
			{
				var projRef = tmp;
				var proj = projRef.Source.Project;

				// Dummy reference for logs in case of errors
				var dep = Dependency.WithLibrary(proj, null, projRef.ReferenceLocation, projRef.ReferenceFilename);

				List<Library> found = null;

				if (projRef.ReferenceFilename != null)
					found = FindLibraryByFilename(proj, dep, projRef.ReferenceFilename);

				if (projRef.ReferenceName != null && projRef.ReferenceLibraryName != null)
					found = FindLibraryByNameAndLibraryName(proj, dep, projRef.ReferenceName, projRef.ReferenceLibraryName);

				if (projRef.ReferenceLibraryName != null)
					found = FindLibraryByLibraryName(proj, dep, projRef.ReferenceLibraryName);

				if (projRef.ReferenceName != null)
					found = FindLibraryByName(proj, dep, projRef.ReferenceName);

				if (found == null)
				{
					var lib = new Library(projRef.ReferenceLibraryName ?? projRef.ReferenceName);
					if (!Ignore(lib))
					{
						AddLibrary(lib);
						found = lib.AsList();
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
			public readonly string LibraryName;
			public readonly Guid? Guid;
			public readonly string Filename;
			public readonly HashSet<string> Names = new HashSet<string>();
			public readonly HashSet<string> LibraryNames = new HashSet<string>();

			public Project Project;
			public bool Ignored;

			public TempProject(string name, string libraryName, Guid? guid, string filename)
			{
				Argument.ThrowIfNull(name);
				Argument.ThrowIfNull(libraryName);
				Argument.ThrowIfNull(filename);

				Name = name;
				LibraryName = libraryName;
				Guid = guid;
				Filename = filename;

				Names.Add(name);
				LibraryNames.Add(libraryName);
			}

			private bool Equals(TempProject other)
			{
				return string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase)
				       && string.Equals(LibraryName, other.LibraryName, StringComparison.CurrentCultureIgnoreCase) && Guid.Equals(other.Guid)
				       && string.Equals(Filename, other.Filename, StringComparison.CurrentCultureIgnoreCase);
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
					var hashCode = (Name != null
						? Name.ToLower(CultureInfo.CurrentCulture)
							.GetHashCode()
						: 0);
					hashCode = (hashCode * 397) ^ (LibraryName != null
						? LibraryName.ToLower(CultureInfo.CurrentCulture)
							.GetHashCode()
						: 0);
					hashCode = (hashCode * 397) ^ Guid.GetHashCode();
					hashCode = (hashCode * 397) ^ (Filename != null
						? Filename.ToLower(CultureInfo.CurrentCulture)
							.GetHashCode()
						: 0);
					return hashCode;
				}
			}

			public override string ToString()
			{
				return Name;
			}
		}

		private class TempReference
		{
			public readonly TempProject Source;
			public readonly Dependency.Types Type;
			public readonly string ReferenceName;
			public readonly string ReferenceLibraryName;
			public readonly Guid? ReferenceGuid;
			public readonly string ReferenceFilename;
			public readonly Location ReferenceLocation;

			public TempReference(TempProject source, Dependency.Types type, string referenceName, string referenceLibraryName, Guid? referenceGuid,
				string referenceFilename, Location referenceLocation)
			{
				Argument.ThrowIfNull(source);
				Argument.ThrowIfAllNull(referenceName, referenceLibraryName);
				Argument.ThrowIfNull(referenceLocation);

				Source = source;
				Type = type;
				ReferenceName = referenceName;
				ReferenceLibraryName = referenceLibraryName;
				ReferenceGuid = referenceGuid;
				ReferenceFilename = referenceFilename;
				ReferenceLocation = referenceLocation;
			}

			public override string ToString()
			{
				return string.Format("{0} -> {1}", Source.Name, ReferenceName ?? ReferenceLibraryName);
			}
		}
	}
}
