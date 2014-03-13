using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.config;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter
{
	public class RulesMatcher
	{
		public static List<OutputEntry> Match(DependencyGraph graph, Config config)
		{
			var result = new List<OutputEntry>();

			foreach (var rule in config.Rules)
			{
				var matches = rule.Process(graph);
				if (matches != null)
					result.AddRange(matches);
			}

			var projs = graph.Vertices.ToList();
			projs.Sort(Library.NaturalOrdering);

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
				var match = FindMatch(config.Rules, dep);

				if (match != null)
					result.Add(match);
			}

			return result;
		}

		public static OutputEntry FindMatch(List<Rule> rules, Dependency dep)
		{
			foreach (var rule in rules)
			{
				var match = rule.Process(dep);
				if (match != null)
					return match;
			}

			return null;
		}
	}
}
