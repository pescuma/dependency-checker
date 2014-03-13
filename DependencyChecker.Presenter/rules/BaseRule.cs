using System.Collections.Generic;
using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.output;

namespace org.pescuma.dependencychecker.rules
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

		public virtual List<OutputEntry> Process(DependencyGraph graph, Library proj)
		{
			return null;
		}

		public virtual OutputEntry Process(Dependency dep)
		{
			return null;
		}
	}
}
