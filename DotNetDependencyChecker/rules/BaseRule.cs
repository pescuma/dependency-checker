using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public abstract class BaseRule : Rule
	{
		// HACK [pescuma] Fields are public for tests
		public readonly Severity Severity;
		public readonly ConfigLocation Location;

		protected BaseRule(Severity severity, ConfigLocation location)
		{
			Severity = severity;
			Location = location;
		}

		public virtual List<RuleMatch> Process(DependencyGraph graph)
		{
			return null;
		}

		public virtual List<RuleMatch> Process(DependencyGraph graph, Project proj)
		{
			return null;
		}

		public virtual RuleMatch Process(DependencyGraph graph, Dependency dep)
		{
			return null;
		}
	}
}
