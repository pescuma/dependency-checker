using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public abstract class BaseRule : Rule
	{
		public Severity Severity { get; private set; }
		public ConfigLocation Location { get; private set; }

		protected BaseRule(Severity severity, ConfigLocation location)
		{
			Severity = severity;
			Location = location;
		}

		public virtual List<OutputEntry> Process(DependencyGraph graph)
		{
			return null;
		}

		public virtual List<OutputEntry> Process(DependencyGraph graph, Assembly proj)
		{
			return null;
		}

		public virtual OutputEntry Process(Dependency dep)
		{
			return null;
		}
	}
}
