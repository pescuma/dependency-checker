using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.config
{
	public class ConfigParser
	{
		private const string IGNORE_ALL_NON_LOCAL_PROJECTS = "ignore all non-local projects";

		private const string COMMENT = "#";
		private const string DEPENDS = "->";
		private const string NOT_DEPENDS = "-X->";

		public static Config Parse(string filename)
		{
			filename = Path.GetFullPath(filename);

			if (!File.Exists(filename))
				throw new ConfigParserException("Config file doesn't exits: " + filename);

			var lines = File.ReadAllLines(filename);

			return ParseLines(lines);
		}

		public static Config ParseLines(string[] lines)
		{
			var result = new Config();

			var lineTypes = new Dictionary<string, Action<string, ConfigLocation>>
			{
				{ "input:", (line, location) => ParseInput(result, line) },
				{ "group:", (line, location) => ParseGroup(result, line, location) },
				{ "output projects:", (line, location) => ParseOutputProjects(result, line) },
				{ "output groups:", (line, location) => ParseOutputGroups(result, line) },
				{ "output dependencies:", (line, location) => ParseOutputDependencies(result, line) },
				{ "rule:", (line, location) => ParseRule(result, line, location) },
				{ "ignore:", (line, location) => ParseIgnore(result, line, location) },
				{ IGNORE_ALL_NON_LOCAL_PROJECTS, (line, location) => ParseIgnoreAllNonLocalProjects(result, line, location) },
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

			return result;
		}

		private static void ParseLine(Dictionary<string, Action<string, ConfigLocation>> types, string line, ConfigLocation location)
		{
			var type = types.FirstOrDefault(t => t.Key == "" || line.StartsWith(t.Key));
			if (type.Value == null)
				throw new ConfigParserException(location, "Unknown line");

			type.Value(line.Substring(type.Key.Length)
				.Trim(), location);
		}

		private static void ParseInput(Config result, string line)
		{
			result.Inputs.Add(line);
		}

		private static void ParseGroup(Config result, string line, ConfigLocation location)
		{
			var pos = line.IndexOf(DEPENDS, StringComparison.Ordinal);
			if (pos < 0)
				throw new ConfigParserException(location, "Invalid group (should contain Name " + DEPENDS + " Contents)");

			var name = line.Substring(0, pos)
				.Trim();

			var matchLine = line.Substring(pos + DEPENDS.Length)
				.Trim();

			var matcher = ParseMatcher(matchLine, location);

			result.Groups.Add(new Config.Group(name, matcher, line));
		}

		private static Func<Project, bool> ParseMatcher(string matchLine, ConfigLocation location)
		{
			Func<Project, bool> result = null;

			var lineTypes = new Dictionary<string, Action<string, ConfigLocation>>
			{
				{ "re:", (line, loc) => result = ParseRE(line) },
				{ "path:", (line, loc) => result = ParsePath(line) },
				{ "", (line, loc) => result = ParseSimpleMatch(line) },
			};

			ParseLine(lineTypes, matchLine, location);

			return result;
		}

		private static Func<Project, bool> ParseRE(string line)
		{
			var re = new Regex("^" + line + "$", RegexOptions.IgnoreCase);

			return proj => re.IsMatch(proj.Name);
		}

		private static Func<Project, bool> ParsePath(string line)
		{
			var path = Path.GetFullPath(line);

			return proj => proj.Paths.Any(pp => PathMatches(pp, path));
		}

		private static bool PathMatches(string fullPath, string beginPath)
		{
			return fullPath.Equals(beginPath, StringComparison.CurrentCultureIgnoreCase)
			       || fullPath.StartsWith(beginPath + "\\", StringComparison.CurrentCultureIgnoreCase);
		}

		private static Func<Project, bool> ParseSimpleMatch(string line)
		{
			return proj => line.Equals(proj.Name, StringComparison.CurrentCultureIgnoreCase);
		}

		private static void ParseOutputProjects(Config result, string line)
		{
			result.Output.Projects.Add(Path.GetFullPath(line));
		}

		private static void ParseOutputGroups(Config result, string line)
		{
			result.Output.Groups.Add(Path.GetFullPath(line));
		}

		private static void ParseOutputDependencies(Config result, string line)
		{
			result.Output.Dependencies.Add(Path.GetFullPath(line));
		}

		private static void ParseRule(Config result, string line, ConfigLocation location)
		{
			var severities = new Dictionary<string, Severity>
			{
				{ "info", Severity.Info },
				{ "warning", Severity.Warn },
				{ "error", Severity.Error },
			};

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

			if (line == "don't allow circular dependencies")
			{
				result.Rules.Add(new NoCircularDepenendenciesRule(severity, location));
				return;
			}

			if (ParseRule(result, line, location, NOT_DEPENDS, severity))
				return;

			if (ParseRule(result, line, location, DEPENDS, severity))
				return;

			throw new ConfigParserException(location, "Invalid rule");
		}

		private static bool ParseRule(Config result, string line, ConfigLocation location, string separator, Severity severity)
		{
			var pos = line.IndexOf(separator, StringComparison.Ordinal);
			if (pos < 0)
				return false;

			var left = ParseMatcher(line.Substring(0, pos)
				.Trim(), location);
			var right = ParseMatcher(line.Substring(pos + separator.Length)
				.Trim(), location);

			result.Rules.Add(new DepenendencyRule(severity, left, right, separator == DEPENDS, location));
			return true;
		}

		private static void ParseIgnore(Config result, string line, ConfigLocation location)
		{
			var matcher = ParseMatcher(line, location);

			result.Ignores.Add(new Config.Ignore(matcher, line));
		}

		private static void ParseIgnoreAllNonLocalProjects(Config result, string line, ConfigLocation location)
		{
			if (line != "")
				throw new ConfigParserException(location, "The line has more text than it should");

			result.Ignores.Add(new Config.Ignore(proj => !proj.IsLocal, IGNORE_ALL_NON_LOCAL_PROJECTS));
		}
	}
}
