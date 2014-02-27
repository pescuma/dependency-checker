using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class CircularDependencyRuleMatch : DependencyRuleMatch
	{
		public CircularDependencyRuleMatch(Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> deps)
			: base(false, "Circular dependency", severity, messsage, rule, deps)
		{
		}
	}
}
