using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker
{
	public class RulesMatcher
	{
		public static List<string> Validate(DependencyGraph graph, Config config)
		{
			var result = new List<string>();

			config.Rules.ForEach(r => r.Start(result, graph));

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
				foreach (var rule in config.Rules)
					if (rule.Process(result, dep))
						break;
			}

			config.Rules.ForEach(r => r.Finish(result, graph));

			return result;
		}
	}
}
