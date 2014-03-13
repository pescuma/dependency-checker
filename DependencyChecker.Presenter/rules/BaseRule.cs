using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.output;

namespace org.pescuma.dependencychecker.presenter.rules
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
