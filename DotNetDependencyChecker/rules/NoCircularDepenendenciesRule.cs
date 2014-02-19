using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;
using QuickGraph.Algorithms;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class NoCircularDepenendenciesRule : BaseRule
	{
		public NoCircularDepenendenciesRule(Severity severity, ConfigLocation location)
			: base(severity, location)
		{
		}

		public override List<OutputEntry> Process(DependencyGraph graph)
		{
			var result = new List<OutputEntry>();

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
				projs.Sort(DependableUtils.NaturalOrdering);

				var deps = graph.Edges.Where(e => projsSet.Contains(e.Source) && projsSet.Contains(e.Target));

				var message = new OutputMessage();
				message.Append("Circular dependency found between projects ");

				var first = true;
				foreach (var proj in projs)
				{
					if (first)
						first = false;
					else
						message.Append(", ");

					message.Append(proj, OutputMessage.ProjInfo.Name);
				}

				result.Add(new DependencyRuleMatch(false, Severity, message, this, deps));
			}

			return result;
		}
	}
}
