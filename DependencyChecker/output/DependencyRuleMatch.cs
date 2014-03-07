using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.output
{
	public class DependencyRuleMatch : RuleOutputEntry
	{
		public readonly bool Allowed;

		public DependencyRuleMatch(bool allowed, string type, Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> deps)
			: base(type, severity, messsage, rule, deps.ToList())
		{
			Allowed = allowed;
		}
	}
}
