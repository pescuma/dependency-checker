using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class UniqueProjectRule : BaseRule
	{
		private readonly Func<Project, string> id;
		private readonly Func<Project, string> description;

		public UniqueProjectRule(Func<Project, string> id, Func<Project, string> description, Severity severity, ConfigLocation location)
			: base(severity, location)
		{
			this.id = id;
			this.description = description;
		}

		public override List<OutputEntry> Process(DependencyGraph graph)
		{
			var result = new List<OutputEntry>();

			var same = graph.Vertices.OfType<Project>()
				.GroupBy(v => id(v))
				.Where(g => g.Count() > 1);
			same.ForEach(g =>
			{
				var projs = g.ToList();

				var message = new OutputMessage();
				message.Append(projs.Count())
					.Append(" projects ")
					.Append(description(projs.First()))
					.Append(" found");

				result.Add(new UniqueProjectOutput(Severity, message, Location, projs));
			});

			return result;
		}
	}
}
