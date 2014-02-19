using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public interface Rule
	{
		List<OutputEntry> Process(DependencyGraph graph);

		List<OutputEntry> Process(DependencyGraph graph, Dependable proj);

		/// <returns>null if didn't match</returns>
		OutputEntry Process(Dependency dep);
	}
}
