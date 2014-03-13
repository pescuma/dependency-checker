using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.input
{
	/// <summary>
	/// Search order is: project, library, ignored project.
	/// On searching returns null if not found and an empty list if found in ignored projects.
	/// </summary>
	internal class LibrariesDB
	{
		private readonly Dictionary<string, HashSet<Library>> librariesByName = new Dictionary<string, HashSet<Library>>();
		private readonly Dictionary<string, HashSet<Library>> librariesByFilename = new Dictionary<string, HashSet<Library>>();
		private readonly Dictionary<string, HashSet<Library>> projectsByName = new Dictionary<string, HashSet<Library>>();
		private readonly Dictionary<string, HashSet<Library>> projectsByFilename = new Dictionary<string, HashSet<Library>>();
		private readonly Dictionary<string, HashSet<Library>> ignoredProjectByName = new Dictionary<string, HashSet<Library>>();
		private readonly Dictionary<string, HashSet<Library>> ignoredProjectByFilename = new Dictionary<string, HashSet<Library>>();
		private readonly HashSet<Library> all = new HashSet<Library>();

		public void AddProject(Project project)
		{
			Add(projectsByName, projectsByFilename, project);
		}

		public void AddIgnoredProject(Project project)
		{
			Add(ignoredProjectByName, ignoredProjectByFilename, project);
		}

		public void AddLibrary(Library library)
		{
			Add(librariesByName, librariesByFilename, library);
		}

		private void Add(Dictionary<string, HashSet<Library>> byName, Dictionary<string, HashSet<Library>> byFilename, Library lib)
		{
			foreach (var name in lib.Names)
				GetList(byName, name.ToLower())
					.Add(lib);

			foreach (var name in lib.LibraryNames)
				GetList(byName, name.ToLower())
					.Add(lib);

			foreach (var path in lib.Paths)
				GetList(byFilename, path.ToLower())
					.Add(lib);

			all.Add(lib);
		}

		private HashSet<Library> GetList(Dictionary<string, HashSet<Library>> dict, string key)
		{
			var result = dict.Get(key);

			if (result == null)
			{
				result = new HashSet<Library>();
				dict.Add(key, result);
			}

			return result;
		}

		public bool Contais(Library lib)
		{
			return all.Contains(lib);
		}

		public List<Library> QueryAll()
		{
			return all.ToList();
		}

		public List<Library> FindByPath(string filename)
		{
			return FindBy(projectsByFilename, librariesByFilename, ignoredProjectByFilename, filename,
				p => p.Paths.Any(path => filename.Equals(path, StringComparison.CurrentCultureIgnoreCase)));
		}

		public List<Library> FindByNameAndLibraryName(string name, string libraryName)
		{
			return FindBy(projectsByName, librariesByName, ignoredProjectByName, name,
				p =>
					name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase)
					&& libraryName.Equals(p.LibraryName, StringComparison.CurrentCultureIgnoreCase));
		}

		public List<Library> FindByLibraryName(string libraryName)
		{
			return FindBy(projectsByName, librariesByName, ignoredProjectByName, libraryName,
				p => libraryName.Equals(p.LibraryName, StringComparison.CurrentCultureIgnoreCase));
		}

		public List<Library> FindByName(string name)
		{
			return FindBy(projectsByName, librariesByName, ignoredProjectByName, name,
				p => name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase));
		}

		public List<Library> FindByNameAndGuid(string name, Guid guid)
		{
			return FindBy(projectsByName, librariesByName, ignoredProjectByName, name,
				p => p is Project && name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase) && ((Project) p).Guid == guid);
		}

		private List<Library> FindBy(Dictionary<string, HashSet<Library>> projects, Dictionary<string, HashSet<Library>> libraries,
			Dictionary<string, HashSet<Library>> ignoredProject, string key, Func<Library, bool> predicate)
		{
			key = key.ToLower();

			var result = FindBy(projects, key, predicate);

			if (result == null)
				result = FindBy(libraries, key, predicate);

			if (result == null)
			{
				result = FindBy(ignoredProject, key, predicate);
				if (result != null)
					// The project exists but was ignored
					result.Clear();
			}

			return result;
		}

		private List<Library> FindBy(Dictionary<string, HashSet<Library>> dict, string key, Func<Library, bool> predicate)
		{
			var tmp = dict.Get(key);
			if (tmp == null)
				return null;

			var result = tmp.Where(predicate)
				.ToList();

			if (!result.Any())
				return null;

			return result;
		}
	}
}
