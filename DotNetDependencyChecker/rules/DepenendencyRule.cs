using System;
using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class DepenendencyRule : BaseRule
	{
		// HACK [pescuma] Fields are public for tests
		public readonly Func<Dependable, bool> Source;
		public readonly Func<Dependable, bool> Target;
		public readonly bool Allow;

		public DepenendencyRule(Severity severity, Func<Dependable, bool> source, Func<Dependable, bool> target, bool allow,
			ConfigLocation location)
			: base(severity, location)
		{
			Source = source;
			Target = target;
			Allow = allow;
		}

		public override RuleMatch Process(Dependency dep)
		{
			if (!Source(dep.Source) || !Target(dep.Target))
				return null;

			var messsage = new OutputMessage();
			messsage.Append("Dependence between ")
				.Append(dep.Source, OutputMessage.ProjInfo.NameAndGroup)
				.Append(" and ")
				.Append(dep.Target, OutputMessage.ProjInfo.NameAndGroup)
				.Append(Allow ? "" : " not")
				.Append(" allowed");
			var projs = new List<Dependable> { dep.Source, dep.Target };
			var dependencies = new List<Dependency> { dep };

			return new RuleMatch(Allow, Severity, messsage, Location, projs, dependencies);
		}
	}
}
