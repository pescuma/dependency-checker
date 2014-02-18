using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;
using org.pescuma.dotnetdependencychecker.utils;

namespace org.pescuma.dotnetdependencychecker.config
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
				{ "output dependencies:", ParseOutputDependencies },
				{ "rule:", ParseRule },
				{ "ignore:", ParseIgnore },
				{ "ignore all references not in includes", ParseIgnoreAllNonLocalProjects },
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
			config.Inputs.Add(line);
		}

		private void ParseGroup(string line, ConfigLocation location)
		{
			var pos = line.IndexOf(GROUP_ELEMENT, StringComparison.CurrentCultureIgnoreCase);
			if (pos < 0)
				throw new ConfigParserException(location, "Invalid group (should contain Name " + GROUP_ELEMENT + " Contents)");

			var name = line.Substring(0, pos)
				.Trim();

			var matchLine = line.Substring(pos + DEPENDS.Length)
				.Trim();

			var matcher = ParseMatcher(matchLine, location);

			config.Groups.Add(new Config.Group(name, matcher, line));
		}

		private Func<Dependable, bool> ParseMatcher(string matchLine, ConfigLocation location)
		{
			Func<Dependable, bool> result = null;

			var lineTypes = new Dictionary<string, Action<string, ConfigLocation>>
			{
				{ "re:", (line, loc) => result = ParseRE(line) },
				{ "path:", (line, loc) => result = ParsePath(line) },
				{ "", (line, loc) => result = ParseSimpleMatch(line) },
			};

			ParseLine(lineTypes, matchLine, location);

			return result;
		}

		private Func<Dependable, bool> ParseRE(string line)
		{
			var re = new Regex("^" + line + "$", RegexOptions.IgnoreCase);

			return proj => proj.Names.Any(re.IsMatch);
		}

		private Func<Dependable, bool> ParsePath(string line)
		{
			var path = PathUtils.ToAbsolute(basePath, line);

			return proj => proj.Paths.Any(pp => PathMatches(pp, path));
		}

		private static bool PathMatches(string fullPath, string beginPath)
		{
			return fullPath.Equals(beginPath, StringComparison.CurrentCultureIgnoreCase)
			       || fullPath.StartsWith(beginPath + "\\", StringComparison.CurrentCultureIgnoreCase);
		}

		private Func<Dependable, bool> ParseSimpleMatch(string line)
		{
			return proj => proj.Names.Any(n => line.Equals(n, StringComparison.CurrentCultureIgnoreCase));
		}

		private void ParseOutputProjects(string line, ConfigLocation configLocation)
		{
			config.Output.Projects.Add(PathUtils.ToAbsolute(basePath, line));
		}

		private void ParseOutputGroups(string line, ConfigLocation configLocation)
		{
			config.Output.Groups.Add(PathUtils.ToAbsolute(basePath, line));
		}

		private void ParseOutputDependencies(string line, ConfigLocation configLocation)
		{
			config.Output.Dependencies.Add(PathUtils.ToAbsolute(basePath, line));
		}

		private static readonly Dictionary<string, Severity> SEVERITIES = new Dictionary<string, Severity>
		{
			{ "info", Severity.Info },
			{ "warning", Severity.Warn },
			{ "error", Severity.Error },
		};

		private static readonly Dictionary<string, Func<Severity, ConfigLocation, Rule>> CUSTOM_RULES =
			new Dictionary<string, Func<Severity, ConfigLocation, Rule>>
			{
				{ "don't allow circular dependencies", (s, l) => new NoCircularDepenendenciesRule(s, l) },
				{ "no two projects with same name", (s, l) => new UniqueProjectRule(p => true, p => p.Name, "name", s, l) },
				{ "no two projects with same guid", (s, l) => new UniqueProjectRule(p => p.Guid != null, p => p.Guid.ToString(), "GUID", s, l) },
				{
					"no two projects with same name and guid",
					(s, l) => new UniqueProjectRule(p => p.Guid != null, p => p.Name + "\n" + p.Guid, "name and GUID", s, l)
				},
				{ "avoid same dependency twice", (s, l) => new UniqueDependenciesRule(s, l) },
			};

		private void ParseRule(string line, ConfigLocation location)
		{
			var severity = Severity.Error;
			foreach (var s in SEVERITIES)
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
			if (CUSTOM_RULES.TryGetValue(line.ToLower(), out factory))
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

		private void ParseIgnoreAllNonLocalProjects(string line, ConfigLocation location)
		{
			if (line != "")
				throw new ConfigParserException(location, "The line has more text than it should");

			config.Ignores.Add(new Config.Ignore(
				el => !(el is Project) || !config.Inputs.Any(input => PathMatches(((Project) el).CsprojPath, input)), location));
		}
	}
}
