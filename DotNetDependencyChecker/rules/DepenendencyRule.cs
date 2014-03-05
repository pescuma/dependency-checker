using System;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

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

		public override OutputEntry Process(Dependency dep)
		{
			if (!Matches(Source, dep.Source) || !Matches(Target, dep.Target))
				return null;

			var messsage = new OutputMessage();
			messsage.Append("Dependence between ")
				.Append(dep.Source, OutputMessage.ProjInfo.NameAndGroup)
				.Append(" and ")
				.Append(dep.Target, OutputMessage.ProjInfo.NameAndGroup)
				.Append(Allow ? "" : " not")
				.Append(" allowed");

			return new DependencyRuleMatch(Allow, "Dependency", Severity, messsage, this, dep.AsList());
		}

		private bool Matches(Func<Dependable, bool> test, Dependable el)
		{
			if (test(el))
				return true;

			var proj = (Assembly) el;
			if (proj.GroupElement != null && test(proj.GroupElement))
				return true;

			return false;
		}
	}
}
