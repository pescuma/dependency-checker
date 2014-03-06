using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public interface Rule
	{
		Severity Severity { get; }
		ConfigLocation Location { get; }

		List<OutputEntry> Process(DependencyGraph graph);

		List<OutputEntry> Process(DependencyGraph graph, Library proj);

		/// <returns>null if didn't match</returns>
		OutputEntry Process(Dependency dep);
	}
}
