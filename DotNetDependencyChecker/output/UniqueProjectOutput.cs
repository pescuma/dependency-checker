using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class UniqueProjectOutput : BaseOutputEntry
	{
		public readonly ConfigLocation Location;

		public UniqueProjectOutput(Severity severity, OutputMessage messsage, ConfigLocation location, IEnumerable<Assembly> projs)
			: base("Non unique project", severity, messsage, projs, null)
		{
			Location = location;
		}
	}
}
