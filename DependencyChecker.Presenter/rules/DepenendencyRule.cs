using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.rules
{
	public class DepenendencyRule : BaseRule
	{
		// HACK Fields are public for tests
		public readonly LibraryMatcher Source;
		public readonly LibraryMatcher Target;
		public readonly DependencyMatcher Dependency;
		public readonly bool Allow;

		public DepenendencyRule(Severity severity, LibraryMatcher source, LibraryMatcher target, DependencyMatcher dependency, bool allow,
			ConfigLocation location)
			: base(severity, location)
		{
			Source = source ?? ((l, r) => true);
			Target = target ?? ((l, r) => true);
			Dependency = dependency ?? ((d, r) => true);
			Allow = allow;
		}

		public override OutputEntry Process(Dependency dep)
		{
			var fields = new List<ProcessedField>();

			if (!Dependency(dep, (f, v, m) => fields.Add(new ProcessedField("Dependency " + f, v, m))))
				return null;

			if (!Matches(Source, dep.Source, (f, v, m) => fields.Add(new ProcessedField("Source " + f, v, m))))
				return null;

			if (!Matches(Target, dep.Target, (f, v, m) => fields.Add(new ProcessedField("Target " + f, v, m))))
				return null;

			var messsage = new OutputMessage();
			messsage.Append("Dependency between ")
				.Append(dep.Source, OutputMessage.ProjInfo.Name)
				.Append(" and ")
				.Append(dep.Target, OutputMessage.ProjInfo.Name)
				.Append(Allow ? "" : " not")
				.Append(" allowed");

			return new DependencyRuleMatch(Allow, "Dependency", Severity, messsage, this, dep.AsList(), fields);
		}

		private bool Matches(LibraryMatcher test, Library proj, Matchers.Reporter reporter)
		{
			if (test(proj, (f, v, m) => reporter("Library " + f, v, m)))
				return true;

			if (proj.GroupElement != null && test(proj.GroupElement, (f, v, m) => reporter("Group " + f, v, m)))
				return true;

			return false;
		}
	}
}
