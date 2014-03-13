using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.architecture;

namespace org.pescuma.dependencychecker.presenter.output.dependencies
{
	public interface DependenciesOutputer
	{
		void Output(DependencyGraph graph, ArchitectureGraph architecture, List<OutputEntry> warnings);
	}
}
