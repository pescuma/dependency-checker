using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
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
				var warnings = new List<RuleMatch>();

				var config = new ConfigParser().Parse(args[0]);

				var graph = new ProjectsLoader(config, warnings).LoadGraph();

				DumpProjects(graph.Vertices, config.Output.Projects);

				new GroupsLoader(config, graph).FillGroups();

				DumpGroups(graph.Vertices, config.Output.Groups);

				warnings.AddRange(RulesMatcher.Match(graph, config)
					.Where(e => !e.Allowed));

				if (warnings.Any())
					warnings.ForEach(e => Console.WriteLine("\n[{0}] {1}", e.Severity.ToString()
						.ToLower(), ToConsole(e.Messsage)));
				else
					Console.WriteLine("No errors found");

				Console.WriteLine();
				return 0;
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

		private static string ToConsole(OutputMessage messsage)
		{
			return string.Join("", messsage.Elements.Select(e =>
			{
				if (e.Text != null)
					return e.Text;

				else if (e.Project != null)
					return InfoForConsole(e.Project, e.ProjInfo);

				else if (e.Dependendcy != null)
					return InfoForConsole(e.Dependendcy, e.DepInfo);

				else
					throw new InvalidDataException();
			}));
		}

		private static string InfoForConsole(Dependable proj, OutputMessage.ProjInfo info)
		{
			switch (info)
			{
				case OutputMessage.ProjInfo.Name:
				{
					if (proj.Paths.Any())
						return string.Format("{0} ({1})", string.Join(" or ", proj.Names), string.Join(", ", proj.Paths));
					else
						return string.Join(", ", proj.Names);
				}
				case OutputMessage.ProjInfo.Path:
				{
					if (proj.Paths.Any())
						return string.Join(" or ", proj.Names);
					else
						return string.Join(" or ", proj.Paths);
				}
				default:
					throw new InvalidDataException();
			}
		}

		private static string InfoForConsole(Dependency dep, OutputMessage.DepInfo info)
		{
			switch (info)
			{
				case OutputMessage.DepInfo.Type:
				{
					switch (dep.Type)
					{
						case Dependency.Types.DllReference:
							return "DLL reference";
						case Dependency.Types.ProjectReference:
							return "project reference";
						default:
							throw new InvalidDataException();
					}
				}
				case OutputMessage.DepInfo.Line:
				{
					return "line " + dep.Location.Line;
				}
				default:
					throw new InvalidDataException();
			}
		}

		private static void DumpProjects(IEnumerable<Dependable> projects, List<string> filenames)
		{
			if (!filenames.Any())
				return;

			var projs = projects.ToList();
			projs.Sort(DependableUtils.NaturalOrdering);

			var names = projs.Select(p => InfoForConsole(p, OutputMessage.ProjInfo.Name))
				.ToList();

			filenames.ForEach(f => File.WriteAllLines(f, names));
		}

		private static void DumpGroups(IEnumerable<Dependable> projects, List<string> filenames)
		{
			if (!filenames.Any())
				return;

			var groups = projects.OfType<Assembly>()
				.GroupBy(p => p.Group)
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
				projs.Sort(DependableUtils.NaturalOrdering);
				projs.ForEach(p => result.Append("  - ")
					.Append(InfoForConsole(p, OutputMessage.ProjInfo.Name))
					.Append("\n"));

				result.Append("\n");
			});
			var text = result.ToString();

			filenames.ForEach(f => File.WriteAllText(f, text));
		}
	}
}
