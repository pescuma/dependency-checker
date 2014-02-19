using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

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

		public virtual List<OutputEntry> Process(DependencyGraph graph)
		{
			return null;
		}

		public virtual List<OutputEntry> Process(DependencyGraph graph, Dependable proj)
		{
			return null;
		}

		public virtual OutputEntry Process(Dependency dep)
		{
			return null;
		}
	}
}
