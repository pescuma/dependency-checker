using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.output
{
	public class DependencyRuleMatch : BaseOutputEntry
	{
		public readonly bool Allowed;
		public readonly Rule Rule;

		public DependencyRuleMatch(bool allowed, string type, Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> deps)
			: base(type, severity, messsage, deps.ToList())
		{
			Allowed = allowed;
			Rule = rule;
		}
	}
}
