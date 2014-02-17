using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using QuickGraph.Algorithms;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class NoCircularDepenendenciesRule : BaseRule
	{
		public NoCircularDepenendenciesRule(Severity severity, ConfigLocation location)
			: base(severity, location)
		{
		}

		public override List<RuleMatch> Process(DependencyGraph graph)
		{
			var result = new List<RuleMatch>();

			IDictionary<Dependable, int> components;
			graph.StronglyConnectedComponents(out components);

			var circularDependencies = components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1);

			foreach (var g in circularDependencies)
			{
				var projs = g.Select(i => i.Proj)
					.ToList();

				var projsSet = new HashSet<Dependable>(projs);
				var deps = graph.Edges.Where(e => projsSet.Contains(e.Source) && projsSet.Contains(e.Target))
					.ToList();

				var message = new OutputMessage();
				message.Append("Circular dependency found:");
				projs.Sort(DependableUtils.NaturalOrdering);
				projs.ForEach(p => message.Append("\n  - ")
					.Append(p, OutputMessage.Info.Name));

				result.Add(new RuleMatch(false, Severity, message, Location, projs, deps));
			}

			return result;
		}
	}
}
