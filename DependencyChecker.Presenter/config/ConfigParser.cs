using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.output.architeture;
using org.pescuma.dependencychecker.presenter.output.dependencies;
using org.pescuma.dependencychecker.presenter.output.results;
using org.pescuma.dependencychecker.presenter.rules;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.config
{
	public class ConfigParser
	{
		private const string COMMENT = "#";
		private const string GROUP_ELEMENT = "+=";
		private const string DEPENDS = "->";
		private const string NOT_DEPENDS = "-X->";

		private string basePath = "";
		private Config config;

		public Config Parse(string filename)
		{
			filename = Path.GetFullPath(filename);

			if (!File.Exists(filename))
				throw new ConfigParserException("Config file doesn't exits: " + filename);

			var lines = File.ReadAllLines(filename);

			return ParseLines(filename, lines);
		}

		public Config ParseLines(string filename, string[] lines)
		{
			basePath = Path.GetDirectoryName(filename);
			config = new Config();

			var lineTypes = new Dictionary<string, Action<string, ConfigLocation>>
			{
				{ "input:", ParseInput },
				{ "group:", ParseGroup },
				{ "output projects:", ParseOutputProjects },
				{ "output groups:", ParseOutputGroups },
				{ "output dependencies:", (line, loc) => ParseOutputDependencies(line, loc, false) },
				{ "output dependencies with errors:", (line, loc) => ParseOutputDependencies(line, loc, true) },
				{ "output architecture:", ParseOutputArchitecture },
				{ "output results:", ParseOutputResults },
				{ "rule:", ParseRule },
				{ "ignore:", ParseIgnore },
				{ "in output:", ParseInOutput },
			};

			var re = new Regex(@"(?<![a-zA-Z0-9])" + COMMENT, RegexOptions.IgnoreCase);

			foreach (var item in lines.Indexed())
			{
				// Line number starts in 1
				var location = new ConfigLocation(item.Index + 1, item.Item);
				var line = item.Item.Trim();

				var m = re.Match(line);
				if (m.Success)
					line = line.Substring(0, m.Index)
						.Trim();

				if (string.IsNullOrWhiteSpace(line))
					continue;

				ParseLine(lineTypes, line, location);
			}

			return config;
		}

		private void ParseLine(Dictionary<string, Action<string, ConfigLocation>> types, string line, ConfigLocation location)
		{
			var type = types.FirstOrDefault(t => t.Key == "" || line.StartsWith(t.Key));
			if (type.Value == null)
				throw new ConfigParserException(location, "Unknown line");

			var remaining = line.Substring(type.Key.Length)
				.Trim();

			if (type.Key == "" && remaining.IndexOf(':') >= 0)
				throw new ConfigParserException(location, "Invalid expression");

			try
			{
				type.Value(remaining, location);
			}
			catch (Exception e)
			{
				throw new ConfigParserException(location, e.Message);
			}
		}

		private void ParseInput(string line, ConfigLocation configLocation)
		{
			var absolute = PathUtils.ToAbsolute(basePath, line);
			config.Inputs.Add(PathUtils.RemoveSeparatorAtEnd(absolute));
		}

		private void ParseGroup(string line, ConfigLocation location)
		{
			var pos = line.IndexOf(GROUP_ELEMENT, StringComparison.CurrentCultureIgnoreCase);
			if (pos < 0)
				throw new ConfigParserException(location, "Invalid group (should contain Name " + GROUP_ELEMENT + " Contents)");

			var name = line.Substring(0, pos)
				.Trim();
			if ("<No Group>".Equals(name, StringComparison.CurrentCultureIgnoreCase))
				name = null;

			var matchLine = line.Substring(pos + DEPENDS.Length)
				.Trim();

			var matcher = ParseProjectMatcher(matchLine, location);

			config.Groups.Add(new Config.Group(name, matcher, location));
		}

		public LibraryMatcher ParseProjectMatcher(string matchLine, ConfigLocation location)
		{
			LibraryMatcher result = null;

			var lineTypes = new Dictionary<string, Action<string, ConfigLocation>>
			{
				{ "not:", (line, loc) => result = ParseProjectNotMatcher(line, loc) },
				{
					"local:", (line, loc) => result = Matchers.Projects.Place(true)
						.And(ParseProjectMatcher(line, loc))
				},
				{
					"non local:", (line, loc) => result = Matchers.Projects.Place(false)
						.And(ParseProjectMatcher(line, loc))
				},
				{
					"project:", (line, loc) => result = Matchers.Projects.Type(true)
						.And(ParseProjectMatcher(line, loc))
				},
				{
					"lib:", (line, loc) => result = Matchers.Projects.Type(false)
						.And(ParseProjectMatcher(line, loc))
				},
				{ "regex:", (line, loc) => result = Matchers.Projects.NameRE(line) },
				{ "path:", (line, loc) => result = Matchers.Projects.Path(basePath, line) },
				{ "path regex:", (line, loc) => result = Matchers.Projects.PathRE(line) },
				{ "lang:", (line, loc) => result = Matchers.Projects.Language(line) },
				{ "", (line, loc) => result = Matchers.Projects.Name(line) },
			};

			ParseLine(lineTypes, matchLine, location);

			return result;
		}

		private LibraryMatcher ParseProjectNotMatcher(string line, ConfigLocation loc)
		{
			var matcher = ParseProjectMatcher(line, loc);

			return (library, reporter) => !matcher(library, (f, v, m) => reporter(f, v, !m));
		}

		private DependencyMatcher ParseDependencyMatcher(string detail, ConfigLocation location)
		{
			DependencyMatcher result = null;

			var lineTypes = new Dictionary<string, Action<string, ConfigLocation>>
			{
				{ "not:", (line, loc) => result = ParseDependencyNotMatcher(line, loc) },
				{ "dep:", (line, loc) => result = Matchers.Dependencies.Type(line) },
				{ "dep path:", (line, loc) => result = Matchers.Dependencies.Path(basePath, line) },
				{ "dep path regex:", (line, loc) => result = Matchers.Dependencies.PathRE(line) },
			};

			ParseLine(lineTypes, detail, location);

			return result;
		}

		private DependencyMatcher ParseDependencyNotMatcher(string line, ConfigLocation loc)
		{
			var matcher = ParseDependencyMatcher(line, loc);

			return (library, reporter) => !matcher(library, (f, v, m) => reporter(f, v, !m));
		}

		private void ParseOutputProjects(string line, ConfigLocation location)
		{
			config.Output.Projects.Add(PathUtils.ToAbsolute(basePath, line));
		}

		private void ParseOutputGroups(string line, ConfigLocation location)
		{
			config.Output.Groups.Add(PathUtils.ToAbsolute(basePath, line));
		}

// ReSharper disable once UnusedParameter.Local
		private void ParseOutputDependencies(string line, ConfigLocation location, bool onlyWithMessages)
		{
			var file = PathUtils.ToAbsolute(basePath, line);
			var extension = Path.GetExtension(file) ?? "";

			if (extension.Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
				config.Output.Dependencies.Add(FilterIfNeeded(onlyWithMessages, new XMLDependenciesOutputer(file)));
			else if (extension.Equals(".dot", StringComparison.CurrentCultureIgnoreCase))
				config.Output.Dependencies.Add(FilterIfNeeded(onlyWithMessages, new DotDependenciesOutputer(file)));
			else
				config.Output.Dependencies.Add(FilterIfNeeded(onlyWithMessages, new TextDependenciesOutputer(file)));
		}

		private DependenciesOutputer FilterIfNeeded(bool onlyWithMessages, DependenciesOutputer next)
		{
			if (onlyWithMessages)
				return new OnlyWithMessagesDependenciesOutputer(next);
			else
				return next;
		}

		private void ParseOutputArchitecture(string line, ConfigLocation location)
		{
			var file = PathUtils.ToAbsolute(basePath, line);
			var extension = Path.GetExtension(file) ?? "";

			if (extension.Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
				config.Output.Architecture.Add(new XMLArchitectureOutputer(file));
			else if (extension.Equals(".dot", StringComparison.CurrentCultureIgnoreCase))
				config.Output.Architecture.Add(new DotArchitectureOutputer(file));
			else
				config.Output.Architecture.Add(new TextArchitectureOutputer(file));
		}

		private void ParseOutputResults(string line, ConfigLocation location)
		{
			if (line.Equals("console", StringComparison.CurrentCultureIgnoreCase))
				config.Output.Results.Add(new ConsoleEntryOutputer(false));

			else if (line.Equals("console verbose", StringComparison.CurrentCultureIgnoreCase))
				config.Output.Results.Add(new ConsoleEntryOutputer(true));

			else
			{
				var file = PathUtils.ToAbsolute(basePath, line);
				var extension = Path.GetExtension(file) ?? "";

				if (extension.Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
					config.Output.Results.Add(new XMLEntryOutputer(file));
				else
					config.Output.Results.Add(new TextEntryOutputer(file));
			}
		}

		private readonly Dictionary<string, Severity> severities = new Dictionary<string, Severity>
		{
			{ "info", Severity.Info },
			{ "warning", Severity.Warning },
			{ "error", Severity.Error },
		};

		private readonly Dictionary<string, Func<ConfigLocation, Severity, DependencyMatcher, Rule>> customRules =
			new Dictionary<string, Func<ConfigLocation, Severity, DependencyMatcher, Rule>>
			{
				{ "don't allow circular dependencies", NewNoCircularDepenendenciesRule },
				{ "don't allow self dependencies", (l, s, d) => new NoSelfDependenciesRule(s, d, l) },
				{ "no two projects with same name", NewUniqueNameProjectRule },
				{ "no two projects with same guid", NewUniqueGuidProjectRule },
				{ "no two projects with same name and guid", NewUniqueNameAndGuidProjectRule },
				{ "avoid same dependency twice", (l, s, d) => new UniqueDependenciesRule(s, d, l) },
			};

		private static NoCircularDepenendenciesRule NewNoCircularDepenendenciesRule(ConfigLocation l, Severity s, DependencyMatcher d)
		{
			if (d != null)
				throw new ConfigParserException(l, "No dependency details are possible in project rules");

			return new NoCircularDepenendenciesRule(s, l);
		}

		private static UniqueProjectRule NewUniqueNameAndGuidProjectRule(ConfigLocation l, Severity s, DependencyMatcher d)
		{
			if (d != null)
				throw new ConfigParserException(l, "No dependency details are possible in project rules");

			return new UniqueProjectRule(p => p.ProjectName != null && p.Guid != null, p => p.Name + "\n" + p.Guid,
				p => "named " + p.Name + " and with GUID " + p.Guid, s, l);
		}

		private static UniqueProjectRule NewUniqueGuidProjectRule(ConfigLocation l, Severity s, DependencyMatcher d)
		{
			if (d != null)
				throw new ConfigParserException(l, "No dependency details are possible in project rules");

			return new UniqueProjectRule(p => p.Guid != null, p => p.Guid.ToString(), p => "with GUID " + p.Guid, s, l);
		}

		private static UniqueProjectRule NewUniqueNameProjectRule(ConfigLocation l, Severity s, DependencyMatcher d)
		{
			if (d != null)
				throw new ConfigParserException(l, "No dependency details are possible in project rules");

			return new UniqueProjectRule(p => p.ProjectName != null, p => p.Name, p => "named " + p.Name, s, l);
		}

		private void ParseRule(string line, ConfigLocation location)
		{
			var severity = Severity.Error;
			DependencyMatcher dependencyFilter = null;

			if (line.EndsWith("]"))
			{
				var start = line.LastIndexOf("[", StringComparison.InvariantCulture);
				var details = line.Substring(start + 1, line.Length - start - 2);
				line = line.Substring(0, start)
					.Trim();

				foreach (var detail in details.Split(',')
					.Select(d => d.Trim()))
				{
					Severity tmp;
					if (severities.TryGetValue(detail.ToLower(), out tmp))
						severity = tmp;
					else
						dependencyFilter = dependencyFilter.And(ParseDependencyMatcher(detail, location));
				}
			}

			var factory = customRules.Get(line.ToLower());
			if (factory != null)
			{
				config.Rules.Add(factory(location, severity, dependencyFilter));
				return;
			}

			if (ParseRule(line, location, NOT_DEPENDS, severity, dependencyFilter))
				return;

			if (ParseRule(line, location, DEPENDS, severity, dependencyFilter))
				return;

			throw new ConfigParserException(location, "Invalid rule");
		}

		private bool ParseRule(string line, ConfigLocation location, string separator, Severity severity, DependencyMatcher dependencyFilter)
		{
			var pos = line.IndexOf(separator, StringComparison.Ordinal);
			if (pos < 0)
				return false;

			var left = ParseProjectMatcher(line.Substring(0, pos)
				.Trim(), location);
			var right = ParseProjectMatcher(line.Substring(pos + separator.Length)
				.Trim(), location);

			config.Rules.Add(new DepenendencyRule(severity, left, right, dependencyFilter, separator == DEPENDS, location));

			return true;
		}

		private void ParseIgnore(string line, ConfigLocation location)
		{
			var matcher = ParseProjectMatcher(line, location);

			config.Ignores.Add(new Config.Ignore(matcher, location));
		}

		private void ParseInOutput(string line, ConfigLocation location)
		{
			if (line == "ignore loading infos")
				config.InOutput.Ignore.Add(w => w.Type.StartsWith("Loading/"));

			else if (line == "ignore config infos")
				config.InOutput.Ignore.Add(w => w.Type.StartsWith("Config/"));

			else
				throw new ConfigParserException(location, "Invalid line");
		}
	}
}
