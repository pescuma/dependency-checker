using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public interface Rule
	{
		List<RuleMatch> Process(DependencyGraph graph);

		/// <returns>null if didn't match</returns>
		RuleMatch Process(Dependency dep);
	}
}
