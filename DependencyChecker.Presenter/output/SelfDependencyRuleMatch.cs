﻿using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter.output
{
	public class SelfDependencyRuleMatch : RuleOutputEntry
	{
		public SelfDependencyRuleMatch(Severity severity, OutputMessage messsage, Rule rule, IEnumerable<Dependency> deps)
			: base("Self dependency", severity, messsage, rule, deps)
		{
		}
	}
}
