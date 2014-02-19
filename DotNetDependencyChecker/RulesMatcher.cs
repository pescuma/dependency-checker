using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker
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
			projs.Sort(DependableUtils.NaturalOrdering);

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
				var possibleDeps = GenerateAllPossibleDeps(dep);

				var match = FindMatch(config.Rules, possibleDeps);

				if (match != null)
					result.Add(match);
			}

			return result;
		}

		private static OutputEntry FindMatch(List<Rule> rules, List<Dependency> deps)
		{
			foreach (var rule in rules)
			{
				foreach (var dep in deps)
				{
					var match = rule.Process(dep);
					if (match != null)
						return match;
				}
			}

			return null;
		}

		private static List<Dependency> GenerateAllPossibleDeps(Dependency dep)
		{
			var result = new List<Dependency>();

			result.Add(dep);

			var sourceGroup = GetGroupActingAs(dep.Source);
			var targetGroup = GetGroupActingAs(dep.Target);

			if (sourceGroup != null)
				result.Add(dep.WithSource(sourceGroup));

			if (targetGroup != null)
				result.Add(dep.WithTarget(targetGroup));

			if (sourceGroup != null && targetGroup != null)
				result.Add(dep.WithSource(sourceGroup)
					.WithTarget(targetGroup));

			return result;
		}

		private static Group GetGroupActingAs(Dependable proj)
		{
			var source = proj as Assembly;
			if (source == null)
				return null;

			var result = source.Group;
			if (result == null)
				return null;

			return result.ActingAs(proj);
		}
	}
}
