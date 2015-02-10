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
			if (!Dependency(dep, Matchers.NullReporter))
				return null;

			if (!Matches(Source, dep.Source) || !Matches(Target, dep.Target))
				return null;

			var messsage = new OutputMessage();
			messsage.Append("Dependency between ")
				.Append(dep.Source, OutputMessage.ProjInfo.NameAndGroup)
				.Append(" and ")
				.Append(dep.Target, OutputMessage.ProjInfo.NameAndGroup)
				.Append(Allow ? "" : " not")
				.Append(" allowed");

			return new DependencyRuleMatch(Allow, "Dependency", Severity, messsage, this, dep.AsList());
		}

		private bool Matches(LibraryMatcher test, Library proj)
		{
			if (test(proj, Matchers.NullReporter))
				return true;

			if (proj.GroupElement != null && test(proj.GroupElement, Matchers.NullReporter))
				return true;

			return false;
		}
	}
}
