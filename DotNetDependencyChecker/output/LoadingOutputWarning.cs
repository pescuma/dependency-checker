using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class LoadingOutputWarning : BaseOutputEntry
	{
		public LoadingOutputWarning(OutputMessage messsage, params Dependable[] projects)
			: base(Severity.Warn, messsage, projects, null)
		{
		}

		public LoadingOutputWarning(OutputMessage messsage, params Dependency[] dependencies)
			: base(Severity.Warn, messsage, dependencies)
		{
		}

		public LoadingOutputWarning(OutputMessage messsage)
			: base(Severity.Warn, messsage)
		{
		}
	}
}
