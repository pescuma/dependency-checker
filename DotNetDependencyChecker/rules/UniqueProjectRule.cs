using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;

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

			var same = graph.Vertices.Where(allowProject)
				.GroupBy(v => id(v))
				.Where(g => g.Count() > 1);
			same.ForEach(g =>
			{
				var msg = new StringBuilder();
				msg.Append("Projects with same ")
					.Append(attributes)
					.Append(" found:");
				g.ForEach(e => msg.Append("\n  - ")
					.Append(e.GetNameAndPath()));

				result.Add(new RuleMatch(false, Severity, msg.ToString(), Location, g, null));
			});

			return result;
		}
	}
}
