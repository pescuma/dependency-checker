using System;
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

			var deps = graph.Edges.ToList();
			deps.Sort((d1, d2) =>
			{
				var comp = string.Compare(d1.Source.Name, d2.Source.Name, StringComparison.CurrentCultureIgnoreCase);
				if (comp != 0)
					return comp;

				return string.Compare(d1.Target.Name, d2.Target.Name, StringComparison.CurrentCultureIgnoreCase);
			});

			foreach (var dep in deps)
			{
				var rule = config.Rules.FirstOrDefault(r => r.Source(dep.Source) && r.Target(dep.Target));

				if (rule != null && !rule.Allow)
					result.Add(string.Format("Dependence between {0} and {1} not allowed", dep.Source.ToGui(), dep.Target.ToGui()));
			}

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
