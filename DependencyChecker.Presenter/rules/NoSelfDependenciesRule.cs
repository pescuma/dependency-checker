using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.rules
{
	public class NoSelfDependenciesRule : BaseRule
	{
		public NoSelfDependenciesRule(Severity severity, ConfigLocation location)
			: base(severity, location)
		{
		}

		public override List<OutputEntry> Process(DependencyGraph graph)
		{
			var result = new List<OutputEntry>();

			graph.Edges.Where(e => e.Source.Equals(e.Target))
				.GroupBy(e => e.Source)
				.ForEach(deps =>
				{
					var message = new OutputMessage();
					message.Append(deps.Key, OutputMessage.ProjInfo.Name)
						.Append(" depends on itself");
					result.Add(new SelfDependencyRuleMatch(Severity, message, this, deps));
				});

			return result;
		}
	}
}
