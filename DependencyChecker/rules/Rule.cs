using System.Collections.Generic;
using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.output;

namespace org.pescuma.dependencychecker.rules
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
