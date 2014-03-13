using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter.output
{
	public class CircularDependencyRuleMatch : RuleOutputEntry
	{
		public CircularDependencyRuleMatch(Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> deps)
			: base("Circular dependency", severity, messsage, rule, deps)
		{
		}
	}
}
