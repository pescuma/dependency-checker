using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.rules
{
	public delegate bool LibraryMatcher(Library library, Matchers.Reporter reporter);

	/// <returns>null means that doesn't apply to this library</returns>
	public delegate bool? InternediaryLibraryMatcher(Library library, Matchers.Reporter reporter);

	public delegate bool DependencyMatcher(Dependency library, Matchers.Reporter reporter);

	/// <returns>null means that doesn't apply to this depednency</returns>
	public delegate bool? InternediaryDependencyMatcher(Dependency library, Matchers.Reporter reporter);

	public static class Matchers
	{
		public delegate void Reporter(string field, string value, bool matched);

		public static Reporter NullReporter = (a, b, c) => { };

		internal static class Projects
		{
			public static InternediaryLibraryMatcher Name(string names)
			{
				if (names == "*")
					return (p, r) => true;

				List<Func<string, bool>> candidates = CreateStringMatchers(names);

				return (proj, reporter) => proj.Names.Any(n =>
				{
					var result = candidates.Any(c => c(n));

					reporter("Name", n, result);

					return result;
				});
			}

			public static InternediaryLibraryMatcher NameRE(string nameRE)
			{
				if (nameRE == ".*")
					return (p, r) => true;

				var re = new Regex("^" + nameRE + "$", RegexOptions.IgnoreCase);

				return (proj, reporter) => proj.Names.Any(n =>
				{
					var result = re.IsMatch(n);

					reporter("Name", n, result);

					return result;
				});
			}

			public static InternediaryLibraryMatcher Path(string basePath, string paths)
			{
				var candidates = paths.Split('|')
					.Select(p => PathUtils.ToAbsolute(basePath, p))
					.ToList();

				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return null;

					return proj.Paths.Any(p =>
					{
						var result = candidates.Any(c => PathUtils.PathMatches(p, c));

						reporter("Path", p, result);

						return result;
					});
				};
			}

			public static InternediaryLibraryMatcher PathRE(string pathRE)
			{
				if (pathRE == ".*")
					return (p, r) => true;

				var re = new Regex("^" + pathRE + "$", RegexOptions.IgnoreCase);

				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return null;

					return proj.Paths.Any(p =>
					{
						var result = re.IsMatch(p);

						reporter("Path", p, result);

						return result;
					});
				};
			}

			public static InternediaryLibraryMatcher Language(string languages)
			{
				var candidates = CreateStringMatchers(languages);

				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return null;

					return proj.Languages.Any(l =>
					{
						var result = candidates.Any(c => c(l));

						reporter("Language", l, result);

						return result;
					});
				};
			}

			public static InternediaryLibraryMatcher Place(bool local)
			{
				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return null;

					var result = proj.IsLocal == local;

					reporter("Place", proj.IsLocal ? "Local" : "Non Local", result);

					return result;
				};
			}

			public static InternediaryLibraryMatcher Type(bool project)
			{
				return (proj, reporter) =>
				{
					if (proj is GroupElement)
						return null;

					var result = (proj is Project) == project;

					reporter("Type", proj is Project ? "Project" : "Library", result);

					return result;
				};
			}
		}

		internal static class Dependencies
		{
			public static InternediaryDependencyMatcher Type(string type)
			{
				bool library;

				if ("library".Equals(type, StringComparison.InvariantCultureIgnoreCase))
					library = true;
				else if ("project".Equals(type, StringComparison.InvariantCultureIgnoreCase))
					library = false;
				else
					throw new ArgumentException("Invalid dependency type: " + type);

				return (dependency, reporter) =>
				{
					var result = (dependency.Type == Dependency.Types.LibraryReference) == library;

					reporter("Type", dependency.Type == Dependency.Types.LibraryReference ? "Library reference" : "Project reference", result);

					return result;
				};
			}

			public static InternediaryDependencyMatcher Path(string basePath, string paths)
			{
				var pathsArray = paths.Split('|');

				var matchEmpty = pathsArray.Contains("") || pathsArray.Contains("<empty>");

				var candidates = pathsArray.Where(p => p != "" && p != "<empty>")
					.Select(p => PathUtils.ToAbsolute(basePath, p))
					.ToList();

				return (d, reporter) =>
				{
					if (d.Type == Dependency.Types.ProjectReference)
						return null;

					if (d.ReferencedPath == null)
					{
						reporter("Path", "<empty>", matchEmpty);

						return matchEmpty;
					}
					else
					{
						var result = candidates.Any(c => PathUtils.PathMatches(d.ReferencedPath, c));

						reporter("Path", d.ReferencedPath, result);

						return result;
					}
				};
			}

			public static InternediaryDependencyMatcher PathRE(string pathRE)
			{
				if (pathRE == ".*")
					return (d, r) => true;

				var re = new Regex("^" + pathRE + "$", RegexOptions.IgnoreCase);

				return (d, reporter) =>
				{
					if (d.Type == Dependency.Types.ProjectReference)
						return null;

					var result = re.IsMatch(d.ReferencedPath ?? "");

					reporter("Path", d.ReferencedPath ?? "<empty>", result);

					return result;
				};
			}
		}

		public static List<Func<string, bool>> CreateStringMatchers(string names)
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
		public static InternediaryLibraryMatcher And(this InternediaryLibraryMatcher p1, InternediaryLibraryMatcher p2)
		{
			if (p1 == null)
				return p2;

			if (p2 == null)
				return p1;

			return (a1, a2) =>
			{
				var r1 = p1(a1, a2);
				if (r1 == null)
					return null;

				var r2 = p2(a1, a2);
				if (r2 == null)
					return null;

				return r1.Value && r2.Value;
			};
		}

		public static DependencyMatcher And(this DependencyMatcher p1, DependencyMatcher p2)
		{
			if (p1 == null)
				return p2;

			if (p2 == null)
				return p1;

			return (a1, a2) => p1(a1, a2) && p2(a1, a2);
		}

		public static LibraryMatcher Finalize(this InternediaryLibraryMatcher m)
		{
			return (l, r) => m(l, r) ?? false;
		}

		public static DependencyMatcher Finalize(this InternediaryDependencyMatcher m)
		{
			return (d, r) => m(d, r) ?? false;
		}
	}
}
