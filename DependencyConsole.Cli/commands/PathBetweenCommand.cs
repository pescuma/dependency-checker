using System;
using System.Collections.Generic;
using System.Text;
using org.pescuma.dependencychecker.model;
using QuickGraph.Algorithms;

namespace org.pescuma.dependencyconsole.commands
{
	internal class PathBetweenCommand : BaseCommand
	{
		public override string Name
		{
			get { return "path between"; }
		}

		protected override void InternalHandle(string args, DependencyGraph graph)
		{
			var argsArr = args.Split(' ');
			if (argsArr.Length != 2)
			{
				Console.WriteLine("Wrong arguments.");
				Console.WriteLine("You must pass 2 projects, separated by space.");
				return;
			}

			foreach (var source in FilterLibs(graph, argsArr[0]))
			{
				foreach (var target in FilterLibs(graph, argsArr[1]))
				{
					OutputPath(graph, source, target);
					OutputPath(graph, target, source);
				}
			}
		}

		private void OutputPath(DependencyGraph graph, Library source, Library target)
		{
			Console.WriteLine("Path(s) between {0} and {1}:", GetName(source), GetName(target));

			var tryGetPaths = graph.ShortestPathsDijkstra(e => 1, source);
			IEnumerable<Dependency> path;
			if (!tryGetPaths(target, out path))
			{
				Console.WriteLine(PREFIX + "No path found");
				return;
			}

			var result = new StringBuilder();
			result.Append(GetName(source));
			foreach (var edge in path)
				result.Append(" -> ")
					.Append(GetName(edge.Target));

			Console.WriteLine(PREFIX + result);
		}
	}
}
