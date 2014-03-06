using System.Collections.Generic;
using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.output
{
	public class UniqueProjectOutput : BaseOutputEntry
	{
		public readonly ConfigLocation Location;

		public UniqueProjectOutput(Severity severity, OutputMessage messsage, ConfigLocation location, IEnumerable<Library> projs)
			: base("Non unique project", severity, messsage, projs, null)
		{
			Location = location;
		}
	}
}
