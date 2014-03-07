using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.output
{
	public class UniqueDependencyOutput : RuleOutputEntry
	{
		public UniqueDependencyOutput(Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> deps)
			: base("Non unique dependency", severity, messsage, rule, deps.ToList())
		{
		}
	}
}
