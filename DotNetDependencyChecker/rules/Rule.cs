using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public interface Rule
	{
		List<RuleMatch> Process(DependencyGraph graph);

		List<RuleMatch> Process(DependencyGraph graph, Dependable proj);

		/// <returns>null if didn't match</returns>
		RuleMatch Process(Dependency dep);
	}
}
