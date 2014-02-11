using System;
using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.config
{
	public class Config
	{
		public readonly List<string> Inputs = new List<string>();
		public readonly List<Group> Groups = new List<Group>();
		public readonly List<Rule> Rules = new List<Rule>();
		public readonly OutputConfig Output = new OutputConfig();

		public class Group
		{
			public readonly string Name;
			public readonly Func<Project, bool> Matches;

			public Group(string name, Func<Project, bool> matches)
			{
				Name = name;
				Matches = matches;
			}
		}

		public class Rule
		{
			public readonly Func<Project, bool> Source;
			public readonly Func<Project, bool> Target;
			public readonly bool Allow;

			public Rule(Func<Project, bool> source, Func<Project, bool> target, bool allow)
			{
				Source = source;
				Target = target;
				this.Allow = allow;
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
