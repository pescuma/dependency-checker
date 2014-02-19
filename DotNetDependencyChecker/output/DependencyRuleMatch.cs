using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class DependencyRuleMatch : BaseOutputEntry
	{
		public readonly bool Allowed;
		public readonly Rule Rule;

		public DependencyRuleMatch(bool allowed, Severity severity, OutputMessage messsage, Rule rule, params Dependency[] dependencies)
			: base(severity, messsage, dependencies)
		{
			Allowed = allowed;
			Rule = rule;
		}

		public DependencyRuleMatch(bool allowed, Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependable> projects,
			IEnumerable<Dependency> dependencies)
			: base(severity, messsage, projects, dependencies)
		{
			Allowed = allowed;
			Rule = rule;
		}
	}
}
