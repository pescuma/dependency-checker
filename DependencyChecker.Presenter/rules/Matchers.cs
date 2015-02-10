using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.rules
{
	public delegate bool LibraryMatcher(Library library, Matchers.Reporter reporter);

	public delegate bool DependencyMatcher(Dependency library, Matchers.Reporter reporter);

	public static class Matchers
	{
		public delegate void Reporter(string field, string value, bool matched);

		public static Reporter NullReporter = (a, b, c) => { };

		internal static class Projects
		{
			public static LibraryMatcher Name(string names)
			{
				var candidates = CreateStringMatchers(names);

				return (proj, reporter) => proj.Names.Any(n =>
				{
					var result = candidates.Any(c => c(n));

					reporter("Name", n, result);

					return result;
				});
			}

			public static LibraryMatcher NameRE(string nameRE)
			{
				var re = new Regex("^" + nameRE + "$", RegexOptions.IgnoreCase);

				return (proj, reporter) => proj.Names.Any(n =>
				{
					var result = re.IsMatch(n);

					reporter("Name", n, result);

					return result;
				});
			}

			public static LibraryMatcher Path(string basePath, string paths)
			{
				var candidates = paths.Split('|')
					.Select(p => PathUtils.ToAbsolute(basePath, p))
					.ToList();

				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return false;

					return proj.Paths.Any(p =>
					{
						var result = candidates.Any(c => PathUtils.PathMatches(p, c));

						reporter("Path", p, result);

						return result;
					});
				};
			}

			public static LibraryMatcher PathRE(string pathRE)
			{
				var re = new Regex("^" + pathRE + "$", RegexOptions.IgnoreCase);

				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return false;

					return proj.Paths.Any(p =>
					{
						var result = re.IsMatch(p);

						reporter("Path", p, result);

						return result;
					});
				};
			}

			public static LibraryMatcher Language(string languages)
			{
				var candidates = CreateStringMatchers(languages);

				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return false;

					return proj.Languages.Any(l =>
					{
						var result = candidates.Any(c => c(l));

						reporter("Language", l, result);

						return result;
					});
				};
			}

			public static LibraryMatcher Place(bool local)
			{
				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return false;

					var result = proj.IsLocal == local;

					reporter("Place", proj.IsLocal ? "Local" : "Non Local", result);

					return result;
				};
			}

			public static LibraryMatcher Type(bool project)
			{
				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return false;

					var result = (proj is Project) == project;

					reporter("Type", proj is Project ? "Project" : "Library", result);

					return result;
				};
			}
		}

		internal static class Dependencies
		{
			public static DependencyMatcher Type(string type)
			{
				bool library;

				if ("library".Equals(type, StringComparison.InvariantCultureIgnoreCase))
					library = true;
				else if ("project".Equals(type, StringComparison.InvariantCultureIgnoreCase))
					library = false;
				else
					throw new ArgumentException("Invalid dependency type: " + type);

				return (d, reporter) => (d.Type == Dependency.Types.LibraryReference) == library;
			}

			public static DependencyMatcher Path(string basePath, string paths)
			{
				var candidates = paths.Split('|')
					.Select(p => PathUtils.ToAbsolute(basePath, p))
					.ToList();

				return (d, reporter) => d.ReferencedPath != null && candidates.Any(c => PathUtils.PathMatches(d.ReferencedPath, c));
			}

			public static DependencyMatcher PathRE(string pathRE)
			{
				var re = new Regex("^" + pathRE + "$", RegexOptions.IgnoreCase);

				return (d, reporter) => d.ReferencedPath != null && re.IsMatch(d.ReferencedPath);
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

	internal static class MatchersExtensions
	{
		public static LibraryMatcher And(this LibraryMatcher p1, LibraryMatcher p2)
		{
			if (p1 == null)
				return p2;

			if (p2 == null)
				return p1;

			return (a1, a2) => p1(a1, a2) && p2(a1, a2);
		}

		public static LibraryMatcher Or(this LibraryMatcher p1, LibraryMatcher p2)
		{
			if (p1 == null)
				return p2;

			if (p2 == null)
				return p1;

			return (a1, a2) => p1(a1, a2) || p2(a1, a2);
		}

		public static DependencyMatcher And(this DependencyMatcher p1, DependencyMatcher p2)
		{
			if (p1 == null)
				return p2;

			if (p2 == null)
				return p1;

			return (a1, a2) => p1(a1, a2) && p2(a1, a2);
		}

		public static DependencyMatcher Or(this DependencyMatcher p1, DependencyMatcher p2)
		{
			if (p1 == null)
				return p2;

			if (p2 == null)
				return p1;

			return (a1, a2) => p1(a1, a2) || p2(a1, a2);
		}
	}
}
