using System;
using System.Collections.Generic;
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

			var graph = ProjectsLoader.LoadGraph(config);

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
	}
}
