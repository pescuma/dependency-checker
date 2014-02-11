using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using QuickGraph.Algorithms;

namespace org.pescuma.dotnetdependencychecker
{
	public class RulesMatcher
	{
		public static List<string> Validate(DependencyGraph graph, Config config)
		{
			var result = new List<string>();

			if (config.DontAllowCircularDependencies)
				ValidateCircularReferences(result, graph);

			return result;
		}

		private static void ValidateCircularReferences(List<string> result, DependencyGraph graph)
		{
			IDictionary<Project, int> components;
			graph.StronglyConnectedComponents(out components);

			var circularDependencies = components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1);

			foreach (var g in circularDependencies)
			{
				var err = new StringBuilder();
				err.Append("Circular dependency group:");
				g.ForEach(p => err.Append("\n  - ")
					.Append(p.Proj.Name));

				result.Add(err.ToString());
			}
		}
	}
}
