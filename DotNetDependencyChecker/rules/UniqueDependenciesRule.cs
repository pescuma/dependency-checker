using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class UniqueDependenciesRule : BaseRule
	{
		public UniqueDependenciesRule(Severity severity, ConfigLocation location)
			: base(severity, location)
		{
		}

		public override List<RuleMatch> Process(DependencyGraph graph, Project proj)
		{
			var result = new List<RuleMatch>();

			var same = graph.OutEdges(proj)
				.GroupBy(d => d.Target.AssemblyName)
				.Where(g => g.Count() > 1);

			same.ForEach(g =>
			{
				var msg = new StringBuilder();
				msg.Append("The project ")
					.Append(proj.GetNameAndPath())
					.Append(" has multiple dependencies on the same assembly:");
				g.ForEach(d => msg.Append("\n  - ")
					.Append(d.Target.GetNameAndPath()));

				result.Add(new RuleMatch(false, Severity, msg.ToString(), Location, proj.AsList(), g));
			});

			return result;
		}
	}
}
