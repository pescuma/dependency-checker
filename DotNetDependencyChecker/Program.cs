using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Use: dotnet-dependency-checker <config file>");
				Console.WriteLine();
				return -1;
			}

			try
			{
				var warnings = new List<OutputEntry>();

				var config = new ConfigParser().Parse(args[0]);

				var graph = new ProjectsLoader(config, warnings).LoadGraph();

				DumpProjects(graph.Vertices, config.Output.Projects);

				new GroupsLoader(config, graph).FillGroups();

				DumpGroups(graph.Vertices, config.Output.Groups);

				warnings.AddRange(RulesMatcher.Match(graph, config));

				warnings = warnings.Where(e => !(e is DependencyRuleMatch) || !((DependencyRuleMatch) e).Allowed)
					.Where(w => !config.InOutput.Ignore.Any(f => f(w)))
					.ToList();

				config.Output.Results.ForEach(o => o.Output(warnings));

				config.Output.Dependencies.ForEach(o => o.Output(graph, warnings));

				DumpNumberOfErrorsFound(warnings);
				Console.WriteLine();
				return warnings.Count(w => w.Severity == Severity.Error);
			}
			catch (ConfigParserException e)
			{
				Console.WriteLine("Error parsing config file: " + e.Message);
				Console.WriteLine();
				return -1;
			}
			catch (ConfigException e)
			{
				Console.WriteLine("Error: " + e.Message);
				Console.WriteLine();
				return -1;
			}
		}

		private static void DumpNumberOfErrorsFound(List<OutputEntry> warnings)
		{
			if (!warnings.Any())
			{
				Console.WriteLine("No errors found");
				return;
			}

			var gs = warnings.GroupBy(w => w.Severity)
				.Select(e => new { Severity = e.Key, Count = e.Count() })
				.ToList();
			gs.Sort((s1, s2) => (int) s2.Severity - (int) s1.Severity);

			Console.WriteLine("Found " + string.Join(", ", gs.Select(e => e.Count + " " + e.Severity.ToString()
				.ToLower() + "(s)")));
		}

		private static void DumpProjects(IEnumerable<Assembly> projects, List<string> filenames)
		{
			if (!filenames.Any())
				return;

			var projs = projects.ToList();
			projs.Sort(Assembly.NaturalOrdering);

			var names = projs.Select(p => string.Join(" or ", p.Names))
				.ToList();

			filenames.ForEach(f => File.WriteAllLines(f, names));
		}

		private static void DumpGroups(IEnumerable<Assembly> projects, List<string> filenames)
		{
			if (!filenames.Any())
				return;

			var groups = projects.GroupBy(p => p.GroupElement)
				.ToList();

			groups.Sort((e1, e2) =>
			{
				var g1 = e1.Key;
				var g2 = e2.Key;

				if (Equals(g1, g2))
					return 0;

				if (g1 == null)
					return 1;

				if (g2 == null)
					return -1;

				return string.Compare(g1.Name, g2.Name, StringComparison.CurrentCultureIgnoreCase);
			});

			var result = new StringBuilder();
			groups.ForEach(g =>
			{
				result.Append(g.Key != null ? g.Key.Name : "Without a group")
					.Append(":\n");

				var projs = g.ToList();
				projs.Sort(Assembly.NaturalOrdering);
				projs.ForEach(p => result.Append("  - ")
					.Append(string.Join(" or ", p.Names))
					.Append("\n"));

				result.Append("\n");
			});
			var text = result.ToString();

			filenames.ForEach(f => File.WriteAllText(f, text));
		}
	}
}
