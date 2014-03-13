using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter.output
{
	public class UniqueDependencyOutputEntry : RuleOutputEntry
	{
		public UniqueDependencyOutputEntry(Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> deps)
			: base("Non unique dependency", severity, messsage, rule, deps.ToList())
		{
		}
	}
}
