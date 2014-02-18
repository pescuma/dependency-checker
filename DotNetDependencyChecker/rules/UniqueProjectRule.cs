using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class UniqueProjectRule : BaseRule
	{
		private readonly Func<Project, bool> allowProject;
		private readonly Func<Project, string> id;
		private readonly string attributes;

		public UniqueProjectRule(Func<Project, bool> allowProject, Func<Project, string> id, string attributes, Severity severity,
			ConfigLocation location)
			: base(severity, location)
		{
			this.allowProject = allowProject;
			this.id = id;
			this.attributes = attributes;
		}

		public override List<RuleMatch> Process(DependencyGraph graph)
		{
			var result = new List<RuleMatch>();

			var same = graph.Vertices.OfType<Project>()
				.Where(allowProject)
				.GroupBy(v => id(v))
				.Where(g => g.Count() > 1);
			same.ForEach(g =>
			{
				var message = new OutputMessage();
				message.Append("Projects with same ")
					.Append(attributes)
					.Append(" found:");
				g.ForEach(e => message.Append("\n  - ")
					.Append(e, OutputMessage.ProjInfo.NameAndCsproj));

				result.Add(new RuleMatch(false, Severity, message, Location, g, null));
			});

			return result;
		}
	}
}
