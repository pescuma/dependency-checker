using System;
using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker
{
	public class DepenendencyRule : Rule
	{
		// HACK [pescuma] Fields are public for tests
		public readonly Func<Project, bool> Source;
		public readonly Func<Project, bool> Target;
		public readonly bool Allow;
		private readonly ConfigLocation location;

		public DepenendencyRule(Func<Project, bool> source, Func<Project, bool> target, bool allow, ConfigLocation location)
		{
			Source = source;
			Target = target;
			Allow = allow;
			this.location = location;
		}

		public List<RuleMatch> Process(DependencyGraph graph)
		{
			return null;
		}

		public RuleMatch Process(Dependency dep)
		{
			if (!Source(dep.Source) || !Target(dep.Target))
				return null;

			return new RuleMatch(Allow,
				string.Format("Dependence between {0} and {1} {2}allowed", dep.Source.ToGui(), dep.Target.ToGui(), Allow ? "" : "not "), location, dep);
		}
	}
}
