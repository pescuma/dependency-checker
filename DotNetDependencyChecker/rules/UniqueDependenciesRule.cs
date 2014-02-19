using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class UniqueDependenciesRule : BaseRule
	{
		public UniqueDependenciesRule(Severity severity, ConfigLocation location)
			: base(severity, location)
		{
		}

		public override List<OutputEntry> Process(DependencyGraph graph, Dependable element)
		{
			var proj = element as Project;
			if (proj == null)
				return null;

			var result = new List<OutputEntry>();

			var same = graph.OutEdges(proj)
				.Where(d => d.Target is Project)
				.GroupBy(d => ((Project) d.Target).AssemblyName)
				.Where(g => g.Count() > 1);

			same.ForEach(g =>
			{
				var deps = g.ToList();
				deps.Sort(Dependency.NaturalOrdering);

				var message = new OutputMessage();
				message.Append("The project ")
					.Append(proj, OutputMessage.ProjInfo.NameAndCsproj)
					.Append(" has multiple dependencies on the same assembly:");
				g.ForEach(d => message.Append("\n  - ")
					.Append(d, OutputMessage.DepInfo.Type)
					.Append(" with ")
					.Append(d.Target, OutputMessage.ProjInfo.NameAndCsproj)
					.Append(" in ")
					.Append(d, OutputMessage.DepInfo.Line));

				result.Add(new DependencyRuleMatch(false, Severity, message, this, proj.AsList(), g));
			});

			return result;
		}
	}
}
