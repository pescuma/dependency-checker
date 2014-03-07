using System.Collections.Generic;
using org.pescuma.dependencychecker.architecture;
using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencychecker.output.dependencies
{
	public interface DependenciesOutputer
	{
		void Output(DependencyGraph graph, ArchitectureGraph architecture, List<OutputEntry> warnings);
	}
}
