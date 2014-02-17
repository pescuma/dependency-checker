﻿using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class UniqueDependenciesRule : BaseRule
	{
		public UniqueDependenciesRule(Severity severity, ConfigLocation location)
			: base(severity, location)
		{
		}

		public override List<RuleMatch> Process(DependencyGraph graph, Dependable element)
		{
			var proj = element as Project;
			if (proj == null)
				return null;

			var result = new List<RuleMatch>();

			var same = graph.OutEdges(proj)
				.Where(d => d.Target is Project)
				.GroupBy(d => ((Project) d.Target).AssemblyName)
				.Where(g => g.Count() > 1);

			same.ForEach(g =>
			{
				var message = new OutputMessage();
				message.Append("The project ")
					.Append(proj, OutputMessage.Info.Name)
					.Append(" has multiple dependencies on the same assembly:");
				g.ForEach(d => message.Append("\n  - ")
					.Append(d.Target, OutputMessage.Info.Name));

				result.Add(new RuleMatch(false, Severity, message, Location, proj.AsList(), g));
			});

			return result;
		}
	}
}