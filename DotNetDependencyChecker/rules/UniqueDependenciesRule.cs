using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class UniqueDependenciesRule : BaseRule
	{
		public UniqueDependenciesRule(Severity severity, ConfigLocation location)
			: base(severity, location)
		{
		}

		public override List<OutputEntry> Process(DependencyGraph graph, Dependable element)
		{
			var proj = element as Project;
			if (proj == null)
				return null;

			var result = new List<OutputEntry>();

			var same = graph.OutEdges(proj)
				.GroupBy(d => d.Target)
				.Where(g => g.Count() > 1);

			same.ForEach(g =>
			{
				var message = new OutputMessage();
				message.Append("The project ")
					.Append(proj, OutputMessage.ProjInfo.Name)
					.Append(" has multiple dependencies with ")
					.Append(g.Key, OutputMessage.ProjInfo.Name);

				result.Add(new DependencyRuleMatch(false, "Non unique dependency", Severity, message, this, g));
			});

			return result;
		}
	}
}
