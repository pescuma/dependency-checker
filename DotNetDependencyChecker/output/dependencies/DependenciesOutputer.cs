using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.output.dependencies
{
	public interface DependenciesOutputer
	{
		void Output(DependencyGraph graph);
	}
}
