using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using QuickGraph.Algorithms;

namespace org.pescuma.dotnetdependencychecker
{
	public class NoCircularDepenendenciesRule : Rule
	{
		private readonly ConfigLocation location;

		public NoCircularDepenendenciesRule(ConfigLocation location)
		{
			this.location = location;
		}

		public List<RuleMatch> Process(DependencyGraph graph)
		{
			var result = new List<RuleMatch>();

			IDictionary<Project, int> components;
			graph.StronglyConnectedComponents(out components);

			var circularDependencies = components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1);

			foreach (var g in circularDependencies)
			{
				var projs = g.Select(i => i.Proj)
					.ToList();
				projs.Sort(Project.NaturalOrdering);

				var projsSet = new HashSet<Project>(projs);
				var deps = graph.Edges.Where(e => projsSet.Contains(e.Source) && projsSet.Contains(e.Target))
					.ToList();
				deps.Sort(Dependency.NaturalOrdering);

				var err = new StringBuilder();
				err.Append("Circular dependency found:");
				projs.ForEach(p => err.Append("\n  - ")
					.Append(p.Name));

				result.Add(new RuleMatch(false, err.ToString(), location, deps));
			}

			return result;
		}

		public RuleMatch Process(Dependency dep)
		{
			return null;
		}
	}
}
