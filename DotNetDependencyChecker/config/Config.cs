using System;
using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.config
{
	public class Config
	{
		public readonly List<string> Inputs = new List<string>();
		public readonly List<GroupRule> Groups = new List<GroupRule>();
		public readonly OutputConfig Output = new OutputConfig();

		public class GroupRule
		{
			public readonly string Name;
			public readonly Func<Project, bool> Matches;

			public GroupRule(string name, Func<Project, bool> matches)
			{
				Name = name;
				Matches = matches;
			}
		}

		public class OutputConfig
		{
			public readonly List<string> LocalProjects = new List<string>();
			public readonly List<string> AllProjects = new List<string>();
			public readonly List<string> Groups = new List<string>();
			public readonly List<string> Dependencies = new List<string>();
		}
	}
}
