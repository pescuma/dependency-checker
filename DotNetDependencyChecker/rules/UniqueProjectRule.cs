﻿using System;
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
		private readonly string attributes;

		public UniqueProjectRule(Func<Project, string> id, string attributes, Severity severity, ConfigLocation location)
			: base(severity, location)
		{
			this.id = id;
			this.attributes = attributes;
		}

		public override List<OutputEntry> Process(DependencyGraph graph)
		{
			var result = new List<OutputEntry>();

			var same = graph.Vertices.OfType<Project>()
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

				result.Add(new UniqueProjectOutput(Severity, message, Location));
			});

			return result;
		}
	}
}
