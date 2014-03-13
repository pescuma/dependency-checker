using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.output;

namespace org.pescuma.dependencychecker.presenter.rules
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
