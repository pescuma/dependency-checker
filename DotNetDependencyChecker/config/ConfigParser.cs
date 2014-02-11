﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace org.pescuma.dotnetdependencychecker.config
{
	public class ConfigParser
	{
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

			var lineTypes = new Dictionary<string, Action<string>>
			{
				{ "input:", line => ParseInput(result, line) },
				{ "group:", line => ParseGroup(result, line) },
				{ "output local projects:", line => ParseOutputLocalProjects(result, line) },
				{ "output all projects:", line => ParseOutputAllLocalProjects(result, line) },
				{ "output groups:", line => ParseOutputGroups(result, line) },
				{ "output dependencies:", line => ParseOutputDependencies(result, line) },
			};

			lines.Select(l => l.Trim())
				.Where(l => !string.IsNullOrEmpty(l))
				.ForEach(line => ParseLine(lineTypes, line));

			return result;
		}

		private static void ParseLine(Dictionary<string, Action<string>> types, string line)
		{
			foreach (var type in types)
			{
				if (type.Key != "" && !line.StartsWith(type.Key))
					continue;

				type.Value(line.Substring(type.Key.Length)
					.Trim());

				return;
			}

			throw new ConfigParserException("Unknown line: " + line);
		}

		private static void ParseInput(Config result, string line)
		{
			result.Inputs.Add(line);
		}

		private static void ParseGroup(Config result, string line)
		{
			var pos = line.IndexOf("->", StringComparison.Ordinal);
			if (pos < 0)
				throw new ConfigParserException("Invalid group line (should contain name -> contents): " + line);

			var name = line.Substring(0, pos)
				.Trim();

			var matchLine = line.Substring(pos + 2)
				.Trim();

			var matcher = ParseMatcher(matchLine);

			result.Groups.Add(new Config.GroupRule(name, matcher));
		}

		private static Func<Project, bool> ParseMatcher(string matchLine)
		{
			Func<Project, bool> result = null;

			var lineTypes = new Dictionary<string, Action<string>>
			{
				{ "re:", l => result = ParseRE(l) },
				{ "path:", l => result = ParsePath(l) },
				{ "", l => result = ParseSimpleMatch(l) },
			};

			ParseLine(lineTypes, matchLine);

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
			var pathWithSlash = Path.GetFullPath(line) + "\\";

			return
				proj =>
					path.Equals(proj.Path, StringComparison.CurrentCultureIgnoreCase)
					|| proj.Path.StartsWith(pathWithSlash, StringComparison.CurrentCultureIgnoreCase);
		}

		private static Func<Project, bool> ParseSimpleMatch(string line)
		{
			return proj => line.Equals(proj.Name, StringComparison.CurrentCultureIgnoreCase);
		}

		private static void ParseOutputLocalProjects(Config result, string line)
		{
			result.Output.LocalProjects.Add(Path.GetFullPath(line));
		}

		private static void ParseOutputAllLocalProjects(Config result, string line)
		{
			result.Output.AllProjects.Add(Path.GetFullPath(line));
		}

		private static void ParseOutputGroups(Config result, string line)
		{
			result.Output.Groups.Add(Path.GetFullPath(line));
		}

		private static void ParseOutputDependencies(Config result, string line)
		{
			result.Output.Dependencies.Add(Path.GetFullPath(line));
		}
	}
}
