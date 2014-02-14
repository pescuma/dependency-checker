using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class RulesMatcher
	{
		public static List<RuleMatch> Match(DependencyGraph graph, Config config)
		{
			var result = new List<RuleMatch>();

			foreach (var rule in config.Rules)
			{
				var matches = rule.Process(graph);
				if (matches != null)
					result.AddRange(matches);
			}

			var projs = graph.Vertices.ToList();
			projs.Sort(Project.NaturalOrdering);

			foreach (var proj in projs)
			{
				foreach (var rule in config.Rules)
				{
					var matches = rule.Process(graph, proj);
					if (matches != null)
						result.AddRange(matches);
				}
			}

			var deps = graph.Edges.ToList();
			deps.Sort(Dependency.NaturalOrdering);

			foreach (var dep in deps)
			{
				foreach (var rule in config.Rules)
				{
					var match = rule.Process(graph, dep);
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
