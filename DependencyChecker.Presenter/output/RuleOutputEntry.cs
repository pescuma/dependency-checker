using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter.output
{
	public abstract class RuleOutputEntry : BaseOutputEntry
	{
		public readonly Rule Rule;

		protected RuleOutputEntry(string type, Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Library> projects,
			IEnumerable<Dependency> dependencies)
			: base(type, severity, messsage, projects, dependencies)
		{
			Rule = rule;
		}

		protected RuleOutputEntry(string type, Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> dependencies)
			: base(type, severity, messsage, dependencies)
		{
			Rule = rule;
		}

		protected RuleOutputEntry(string type, Severity severity, OutputMessage messsage, Rule rule)
			: base(type, severity, messsage)
		{
			Rule = rule;
		}
	}
}
