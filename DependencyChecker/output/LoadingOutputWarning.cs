using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.output
{
	public class LoadingOutputWarning : BaseOutputEntry
	{
		public LoadingOutputWarning(string type, OutputMessage messsage, params Dependency[] dependencies)
			: base("Loading/" + type, Severity.Info, messsage, dependencies)
		{
		}
	}
}
