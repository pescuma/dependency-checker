using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using QuickGraph.Algorithms;

namespace org.pescuma.dependencyconsole.commands
{
	internal class CircularDependenciesCommand : BaseCommand
	{
		public override string Name
		{
			get { return "circular dependencies"; }
		}

		protected override void InternalHandle(string args, DependencyGraph graph)
		{
			var projctsFiltered = FilterLibs(graph, args);

			IDictionary<Library, int> components;
			graph.StronglyConnectedComponents(out components);

			var circularDependencies = components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1);

			foreach (var g in circularDependencies)
			{
				var libs = g.Select(i => i.Proj)
					.ToList();

				var projsSet = new HashSet<Library>(libs);

				if (!projsSet.Intersect(projctsFiltered)
					.Any())
					continue;

				libs.Sort(Library.NaturalOrdering);

				Console.WriteLine("Circular dependency:");

				foreach (var lib in libs)
				{
					var proj = lib as Project;
					if (proj != null)
						Console.WriteLine(PREFIX + "{0} (project path: {1})", GetName(proj), proj.ProjectPath);
					else
						Console.WriteLine(PREFIX + GetName(lib));
				}
			}
		}
	}
}
