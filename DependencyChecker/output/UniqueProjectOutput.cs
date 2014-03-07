using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.output
{
	public class UniqueProjectOutput : RuleOutputEntry
	{
		public UniqueProjectOutput(Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Library> projs)
			: base("Non unique project", severity, messsage, rule, projs, null)
		{
		}
	}
}
