using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class DependencyRuleMatch : BaseOutputEntry
	{
		private readonly string type;
		public readonly bool Allowed;
		public readonly Rule Rule;

		public DependencyRuleMatch(bool allowed, string type, Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> deps)
			: base(type, severity, messsage, deps.ToList())
		{
			this.type = type;
			Allowed = allowed;
			Rule = rule;
		}
	}
}
