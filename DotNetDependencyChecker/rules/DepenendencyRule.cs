using System;
using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class DepenendencyRule : Rule
	{
		// HACK [pescuma] Fields are public for tests
		public readonly Severity Severity;
		public readonly Func<Project, bool> Source;
		public readonly Func<Project, bool> Target;
		public readonly bool Allow;
		public readonly ConfigLocation Location;

		public DepenendencyRule(Severity severity, Func<Project, bool> source, Func<Project, bool> target, bool allow, ConfigLocation location)
		{
			Severity = severity;
			Source = source;
			Target = target;
			Allow = allow;
			Location = location;
		}

		public List<RuleMatch> Process(DependencyGraph graph)
		{
			return null;
		}

		public RuleMatch Process(Dependency dep)
		{
			if (!Source(dep.Source) || !Target(dep.Target))
				return null;

			var messsage = string.Format("Dependence between {0} and {1} {2}allowed", dep.Source.Name, dep.Target.Name, Allow ? "" : "not ");
			var projs = new List<Project> { dep.Source, dep.Target };
			var dependencies = new List<Dependency> { dep };

			return new RuleMatch(Allow, Severity, messsage, Location, projs, dependencies);
		}
	}
}
