using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class LoadingOutputWarning : BaseOutputEntry
	{
		public LoadingOutputWarning(OutputMessage messsage, params Dependency[] dependencies)
			: base(Severity.Info, messsage, dependencies)
		{
		}
	}
}
