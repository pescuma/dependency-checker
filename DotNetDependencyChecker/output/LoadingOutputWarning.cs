using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class LoadingOutputWarning : BaseOutputEntry
	{
		public LoadingOutputWarning(string type, OutputMessage messsage, params Dependency[] dependencies)
			: base("Loading/" + type, Severity.Info, messsage, dependencies)
		{
		}
	}
}
