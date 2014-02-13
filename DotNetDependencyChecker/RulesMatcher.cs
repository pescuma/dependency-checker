using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker
{
	public class RulesMatcher
	{
		public static List<RuleMatch> Validate(DependencyGraph graph, Config config)
		{
			var result = new List<RuleMatch>();

			foreach (var rule in config.Rules)
			{
				var matches = rule.Process(graph);
				if (matches != null)
					result.AddRange(matches);
			}

			var deps = graph.Edges.ToList();
			deps.Sort(Dependency.NaturalOrdering);

			foreach (var dep in deps)
			{
				foreach (var rule in config.Rules)
				{
					var match = rule.Process(dep);
					if (match != null)
					{
						result.Add(match);
						break;
					}
				}
			}

			return result;
		}
	}
}
