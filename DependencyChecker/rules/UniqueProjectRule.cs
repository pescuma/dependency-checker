using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.output;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.rules
{
	public class UniqueProjectRule : BaseRule
	{
		private readonly Func<Project, bool> filter;
		private readonly Func<Project, string> id;
		private readonly Func<Project, string> description;

		public UniqueProjectRule(Func<Project, bool> filter, Func<Project, string> id, Func<Project, string> description, Severity severity,
			ConfigLocation location)
			: base(severity, location)
		{
			this.filter = filter;
			this.id = id;
			this.description = description;
		}

		public override List<OutputEntry> Process(DependencyGraph graph)
		{
			var result = new List<OutputEntry>();

			var same = graph.Vertices.OfType<Project>()
				.Where(v => filter(v))
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

				result.Add(new UniqueProjectOutputEntry(Severity, message, this, projs));
			});

			return result;
		}
	}
}
