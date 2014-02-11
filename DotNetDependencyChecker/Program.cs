using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using QuickGraph.Algorithms;

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

			Config config;
			try
			{
				config = ConfigParser.Parse(args[0]);
			}
			catch (ConfigParserException e)
			{
				Console.WriteLine("Error parsing config file: " + e.Message);
				Console.WriteLine();
				return -1;
			}

			var warns = new List<string>();

			var graph = ProjectsLoader.LoadGraph(config, warns);

			warns.ForEach(w => Console.WriteLine("\n[warn] " + w));

			Dump(graph.Vertices.Where(p => p.IsLocal)
				.Select(p => p.Name), config.Output.LocalProjects);

			Dump(graph.Vertices.Select(p => p.Name), config.Output.AllProjects);

			IDictionary<Project, int> components;
			graph.StronglyConnectedComponents(out components);

			var circularDependencies = components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1)
				.ToList();

			if (circularDependencies.Count > 0)
			{
				Console.WriteLine();
				foreach (var g in circularDependencies)
				{
					Console.WriteLine("Circular dependency group:");
					foreach (var p in g)
						Console.WriteLine("  - " + p.Proj.Name);
					Console.WriteLine();
				}
			}

			return 0;
		}

		private static void Dump(IEnumerable<string> projs, List<string> filenames)
		{
			if (!filenames.Any())
				return;

			var names = projs.ToList();

			names.Sort();

			filenames.ForEach(f => File.WriteAllLines(f, names));
		}
	}
}
