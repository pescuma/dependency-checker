using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class UniqueProjectOutput : BaseOutputEntry
	{
		public readonly ConfigLocation Location;

		public UniqueProjectOutput(Severity severity, OutputMessage messsage, ConfigLocation location)
			: base(severity, messsage)
		{
			Location = location;
		}
	}
}
