using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter.output
{
	public class LoadingOutputEntry : BaseOutputEntry
	{
		public LoadingOutputEntry(string type, OutputMessage messsage, params Dependency[] dependencies)
			: base("Loading/" + type, Severity.Info, messsage, dependencies)
		{
		}
	}
}
