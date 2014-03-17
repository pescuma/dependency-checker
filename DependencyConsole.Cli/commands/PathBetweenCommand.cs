﻿using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencyconsole.utils;
using QuickGraph.Algorithms;

namespace org.pescuma.dependencyconsole.commands
{
	internal class PathBetweenCommand : BaseCommand
	{
		public override string Name
		{
			get { return "path between"; }
		}

		protected override void InternalHandle(Output result, string args, DependencyGraph graph)
		{
			var argsArr = args.Split(' ');
			if (argsArr.Length != 2)
			{
				result.AppendLine("Wrong arguments.");
				result.AppendLine("You must pass 2 projects, separated by space.");
				return;
			}

			var libs0 = FilterLibs(graph, argsArr[0]);
			var libs1 = FilterLibs(graph, argsArr[1]);

			if (!libs0.Any() || !libs1.Any())
			{
				if (!libs0.Any())
					result.AppendLine("No projects found matching {0}", argsArr[0]);
				if (!libs1.Any())
					result.AppendLine("No projects found matching {0}", argsArr[1]);
				return;
			}

			foreach (var source in libs0)
			{
				foreach (var target in libs1)
				{
					OutputPath(result, graph, source, target);
					OutputPath(result, graph, target, source);
				}
			}
		}

		private void OutputPath(Output result, DependencyGraph graph, Library source, Library target)
		{
			result.AppendLine("Path(s) between {0} and {1}:", GetName(source), GetName(target));
			result.IncreaseIndent();
			try
			{
				var tryGetPaths = graph.ShortestPathsDijkstra(e => 1, source);
				IEnumerable<Dependency> path;
				if (!tryGetPaths(target, out path))
				{
					result.AppendLine("No path found");
					return;
				}

				var line = result.StartLine();

				line.Append(GetName(source));
				foreach (var edge in path)
					line.Append(" -> ")
						.Append(GetName(edge.Target));

				line.EndLine();
			}
			finally
			{
				result.DecreaseIndent();
				result.AppendLine();
			}
		}
	}
}
