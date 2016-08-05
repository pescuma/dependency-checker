using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.utils;
using QuickGraph.Algorithms;

namespace org.pescuma.dependencyconsole.commands
{
	internal class PathBetweenCommand : BaseCommand
	{
		public override string Name
		{
			get { return "path between"; }
		}

		protected override void InternalHandle(Output result, string anArgs, DependencyGraph graph)
		{
			List<string> args = anArgs.Split(new[] { "->" }, StringSplitOptions.None)
				.Select(e => e.Trim())
				.ToList();
			if (args.Count != 2)
			{
				result.AppendLine("You must specify 2 libraries, separated by ->");
				return;
			}

			List<Library> libs0 = FilterLibs(graph, args[0]);
			List<Library> libs1 = FilterLibs(graph, args[1]);

			if (!libs0.Any() || !libs1.Any())
			{
				if (!libs0.Any())
					result.AppendLine("No projects found matching {0}", args[0]);
				if (!libs1.Any())
					result.AppendLine("No projects found matching {0}", args[1]);
				return;
			}

			bool found = false;
			foreach (Library source in libs0)
			{
				foreach (Library target in libs1)
				{
					found = OutputPath(result, graph, source, target) || found;
					found = OutputPath(result, graph, target, source) || found;
				}
			}

			if (!found)
				result.AppendLine("No path found");
		}

		private bool OutputPath(Output result, DependencyGraph graph, Library source, Library target)
		{
			var tryGetPaths = graph.ShortestPathsDijkstra(e => 1, source);
			IEnumerable<Dependency> path;
			if (!tryGetPaths(target, out path))
				return false;

			try
			{
				result.AppendLine("Path between {0} and {1}:", GetName(source), GetName(target));
				result.IncreaseIndent();

				Output.LineOutput line = result.StartLine();

				line.Append(GetName(source));
				foreach (Dependency edge in path)
					line.Append(" -> ")
						.Append(GetName(edge.Target));

				line.EndLine();
			}
			finally
			{
				result.DecreaseIndent();
				result.AppendLine();
			}

			return true;
		}
	}
}
