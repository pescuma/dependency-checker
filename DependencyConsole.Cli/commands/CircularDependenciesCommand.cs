using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencyconsole.utils;
using QuickGraph.Algorithms;

namespace org.pescuma.dependencyconsole.commands
{
	internal class CircularDependenciesCommand : BaseCommand
	{
		public override string Name
		{
			get { return "circular dependencies"; }
		}

		protected override void InternalHandle(Output result, string args, DependencyGraph graph)
		{
			var projctsFiltered = FilterLibs(graph, args);

			IDictionary<Library, int> components;
			graph.StronglyConnectedComponents(out components);

			var circularDependencies = components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1);

			bool found = false;
			foreach (var g in circularDependencies)
			{
				var libs = g.Select(i => i.Proj)
					.ToList();

				var projsSet = new HashSet<Library>(libs);

				if (!projsSet.Intersect(projctsFiltered)
					.Any())
					continue;

				found = true;

				libs.Sort(Library.NaturalOrdering);

				result.AppendLine("Circular dependency:");
				result.IncreaseIndent();

				foreach (var lib in libs)
				{
					var proj = lib as Project;
					if (proj != null)
						result.AppendLine("{0} (project path: {1})", GetName(proj), proj.ProjectPath);
					else
						result.AppendLine(GetName(lib));
				}

				result.DecreaseIndent();
				result.AppendLine();
			}

			if (!found)
				result.AppendLine("No circular dependency found");
		}
	}
}
