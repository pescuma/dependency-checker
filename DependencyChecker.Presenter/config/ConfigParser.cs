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

			foreach (var item in lines.Indexed())
			{
				// Line number starts in 1
				var location = new ConfigLocation(item.Index + 1, item.Item);
				var line = item.Item.Trim();

				var pos = line.IndexOf(COMMENT, StringComparison.Ordinal);
				if (pos >= 0)
					line = line.Substring(0, pos)
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

			type.Value(line.Substring(type.Key.Length)
				.Trim(), location);
		}

		private void ParseInput(string line, ConfigLocation configLocation)
		{
			config.Inputs.Add(PathUtils.ToAbsolute(basePath, line));
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

			var matcher = ParseMatcher(matchLine, location);

			config.Groups.Add(new Config.Group(name, matcher, location));
		}

		public Func<Library, bool> ParseMatcher(string matchLine, ConfigLocation location)
		{
			Func<Library, bool> result = null;

			var lineTypes = new Dictionary<string, Action<string, ConfigLocation>>
			{
				{ "not:", (line, loc) => result = ParseNot(line, loc) },
				{ "regex:", (line, loc) => result = ParseRE(line) },
				{ "path:", (line, loc) => result = ParsePath(line) },
				{ "lang:", (line, loc) => result = ParseLanguage(line) },
				{ "local:", (line, loc) => result = ParsePlaceAndType(line, loc, true, null) },
				{ "non local:", (line, loc) => result = ParsePlaceAndType(line, loc, false, null) },
				{ "project:", (line, loc) => result = ParsePlaceAndType(line, loc, null, true) },
				{ "local project:", (line, loc) => result = ParsePlaceAndType(line, loc, true, true) },
				{ "non local project:", (line, loc) => result = ParsePlaceAndType(line, loc, false, true) },
				{ "lib:", (line, loc) => result = ParsePlaceAndType(line, loc, null, false) },
				{ "local lib:", (line, loc) => result = ParsePlaceAndType(line, loc, true, false) },
				{ "non local lib:", (line, loc) => result = ParsePlaceAndType(line, loc, false, false) },
				{ "", (line, loc) => result = ParseSimpleMatch(line, loc) },
			};

			ParseLine(lineTypes, matchLine, location);

			return result;
		}

		private Func<Library, bool> ParsePlaceAndType(string line, ConfigLocation loc, bool? local, bool? project)
		{
			var result = ParseMatcher(line, loc);

			if (local != null)
			{
				var inner = result;
				result = l => l.IsLocal == local && inner(l);
			}

			if (project != null)
			{
				var inner = result;
				result = l => (l is Project) == project && inner(l);
			}

			return result;
		}

		private Func<Library, bool> ParseNot(string line, ConfigLocation loc)
		{
			var inner = ParseMatcher(line, loc);

			return lib => !inner(lib);
		}

		private Func<Library, bool> ParseRE(string line)
		{
			var re = new Regex("^" + line + "$", RegexOptions.IgnoreCase);

			return proj => proj.Names.Any(re.IsMatch);
		}

		private Func<Library, bool> ParsePath(string line)
		{
			var path = PathUtils.ToAbsolute(basePath, line);

			return proj => proj.Paths.Any(pp => PathUtils.PathMatches(pp, path));
		}

		private Func<Library, bool> ParseLanguage(string line)
		{
			var m = CreateStringMatcher(line);

			return proj => proj.Languages.Any(m);
		}

		private Func<Library, bool> ParseSimpleMatch(string line, ConfigLocation location)
		{
			if (line.IndexOf(':') >= 0 || line.IndexOf('>') >= 0)
				throw new ConfigParserException(location, "Invalid expression");

			var m = CreateStringMatcher(line);

			return proj => proj.Names.Any(m);
		}

		private Func<string, bool> CreateStringMatcher(string line)
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

		private readonly Dictionary<string, Func<Severity, ConfigLocation, Rule>> customRules =
			new Dictionary<string, Func<Severity, ConfigLocation, Rule>>
			{
				{ "don't allow circular dependencies", (s, l) => new NoCircularDepenendenciesRule(s, l) },
				{ "no two projects with same name", NewUniqueNameProjectRule },
				{ "no two projects with same guid", NewUniqueGuidProjectRule },
				{ "no two projects with same name and guid", NewUniqueNameAndGuidProjectRule },
				{ "avoid same dependency twice", (s, l) => new UniqueDependenciesRule(s, l) },
			};

		private static UniqueProjectRule NewUniqueNameAndGuidProjectRule(Severity s, ConfigLocation l)
		{
			return new UniqueProjectRule(p => p.ProjectName != null && p.Guid != null, p => p.Name + "\n" + p.Guid,
				p => "named " + p.Name + " and with GUID " + p.Guid, s, l);
		}

		private static UniqueProjectRule NewUniqueGuidProjectRule(Severity s, ConfigLocation l)
		{
			return new UniqueProjectRule(p => p.Guid != null, p => p.Guid.ToString(), p => "with GUID " + p.Guid, s, l);
		}

		private static UniqueProjectRule NewUniqueNameProjectRule(Severity s, ConfigLocation l)
		{
			return new UniqueProjectRule(p => p.ProjectName != null, p => p.Name, p => "named " + p.Name, s, l);
		}

		private void ParseRule(string line, ConfigLocation location)
		{
			var severity = Severity.Error;
			foreach (var s in severities)
			{
				var suffix = "[" + s.Key + "]";
				if (line.EndsWith(suffix, StringComparison.CurrentCultureIgnoreCase))
				{
					severity = s.Value;
					line = line.Substring(0, line.Length - suffix.Length)
						.Trim();
					break;
				}
			}

			Func<Severity, ConfigLocation, Rule> factory;
			if (customRules.TryGetValue(line.ToLower(), out factory))
			{
				config.Rules.Add(factory(severity, location));
				return;
			}

			if (ParseRule(line, location, NOT_DEPENDS, severity))
				return;

			if (ParseRule(line, location, DEPENDS, severity))
				return;

			throw new ConfigParserException(location, "Invalid rule");
		}

		private bool ParseRule(string line, ConfigLocation location, string separator, Severity severity)
		{
			var pos = line.IndexOf(separator, StringComparison.Ordinal);
			if (pos < 0)
				return false;

			var left = ParseMatcher(line.Substring(0, pos)
				.Trim(), location);
			var right = ParseMatcher(line.Substring(pos + separator.Length)
				.Trim(), location);

			config.Rules.Add(new DepenendencyRule(severity, left, right, separator == DEPENDS, location));

			return true;
		}

		private void ParseIgnore(string line, ConfigLocation location)
		{
			var matcher = ParseMatcher(line, location);

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
