using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public interface Rule
	{
		List<RuleMatch> Process(DependencyGraph graph);

		List<RuleMatch> Process(DependencyGraph graph, Project proj);

		/// <returns>null if didn't match</returns>
		RuleMatch Process(DependencyGraph graph, Dependency dep);
	}
}
