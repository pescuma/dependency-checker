using System;
using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.output;

namespace org.pescuma.dependencychecker.rules
{
	public class DepenendencyRule : BaseRule
	{
		// HACK [pescuma] Fields are public for tests
		public readonly Func<Library, bool> Source;
		public readonly Func<Library, bool> Target;
		public readonly bool Allow;

		public DepenendencyRule(Severity severity, Func<Library, bool> source, Func<Library, bool> target, bool allow, ConfigLocation location)
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

		private bool Matches(Func<Library, bool> test, Library proj)
		{
			if (test(proj))
				return true;

			if (proj.GroupElement != null && test(proj.GroupElement))
				return true;

			return false;
		}
	}
}
