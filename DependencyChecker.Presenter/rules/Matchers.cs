using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.rules
{
	internal static class Matchers
	{
		internal static class Projects
		{
			public static Func<Library, bool> Name(string names)
			{
				var candidates = CreateStringMatchers(names);

				return proj => proj.Names.Any(n => candidates.Any(c => c(n)));
			}

			public static Func<Library, bool> NameRE(string nameRE)
			{
				var re = new Regex("^" + nameRE + "$", RegexOptions.IgnoreCase);

				return proj => proj.Names.Any(re.IsMatch);
			}

			public static Func<Library, bool> Path(string basePath, string paths)
			{
				var candidates = paths.Split('|')
					.Select(p => PathUtils.ToAbsolute(basePath, p))
					.ToList();

				return proj => !(proj is GroupElement) && proj.Paths.Any(pp => candidates.Any(c => PathUtils.PathMatches(pp, c)));
			}

			public static Func<Library, bool> PathRE(string pathRE)
			{
				var re = new Regex("^" + pathRE + "$", RegexOptions.IgnoreCase);

				return proj => !(proj is GroupElement) && proj.Paths.Any(re.IsMatch);
			}

			public static Func<Library, bool> Language(string languages)
			{
				var candidates = CreateStringMatchers(languages);

				return proj => !(proj is GroupElement) && proj.Languages.Any(l => candidates.Any(c => c(l)));
			}

			public static Func<Library, bool> Place(bool local)
			{
				return proj => !(proj is GroupElement) && proj.IsLocal == local;
			}

			public static Func<Library, bool> Type(bool project)
			{
				return proj => !(proj is GroupElement) && (proj is Project) == project;
			}
		}

		internal static class Dependencies
		{
			public static Func<Dependency, bool> Type(string type)
			{
				if ("library".Equals(type, StringComparison.InvariantCultureIgnoreCase))
					return d => d.Type == Dependency.Types.LibraryReference;

				else if ("project".Equals(type, StringComparison.InvariantCultureIgnoreCase))
					return d => d.Type == Dependency.Types.ProjectReference;

				else
					throw new ArgumentException("Invalid dependency type: " + type);
			}

			public static Func<Dependency, bool> Path(string basePath, string paths)
			{
				var candidates = paths.Split('|')
					.Select(p => PathUtils.ToAbsolute(basePath, p))
					.ToList();

				return d => d.ReferencedPath != null && candidates.Any(c => PathUtils.PathMatches(d.ReferencedPath, c));
			}

			public static Func<Dependency, bool> PathRE(string pathRE)
			{
				var re = new Regex("^" + pathRE + "$", RegexOptions.IgnoreCase);

				return d => d.ReferencedPath != null && re.IsMatch(d.ReferencedPath);
			}
		}

		private static List<Func<string, bool>> CreateStringMatchers(string names)
		{
			return names.Split('|')
				.Select(CreateStringMatcher)
				.ToList();
		}

		private static Func<string, bool> CreateStringMatcher(string line)
		{
			if (line.IndexOf('*') >= 0)
			{
				var pattern = new Regex("^" + line.Replace(".", "\\.")
					.Replace("*", ".*") + "$", RegexOptions.IgnoreCase);

				return pattern.IsMatch;
			}
			else
			{
				return n => line.Equals(n, StringComparison.CurrentCultureIgnoreCase);
			}
		}
	}
}
