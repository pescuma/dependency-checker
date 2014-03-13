using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter.output
{
	public class UniqueProjectOutputEntry : RuleOutputEntry
	{
		public UniqueProjectOutputEntry(Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Library> projs)
			: base("Non unique project", severity, messsage, rule, projs, null)
		{
		}
	}
}
