using System;
using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.config
{
	public class Config
	{
		public readonly List<string> Inputs = new List<string>();
		public readonly List<Group> Groups = new List<Group>();
		public readonly List<Ignore> Ignores = new List<Ignore>();
		public readonly List<Rule> Rules = new List<Rule>();
		public bool DontAllowCircularDependencies;
		public readonly OutputConfig Output = new OutputConfig();

		public class Group
		{
			private readonly string line;
			public readonly string Name;
			public readonly Func<Project, bool> Matches;

			public Group(string name, Func<Project, bool> matches, string line)
			{
				Name = name;
				Matches = matches;
				this.line = line;
			}

			public override string ToString()
			{
				return line;
			}
		}

		public class Ignore
		{
			private readonly string line;
			public readonly Func<Project, bool> Matches;

			public Ignore(Func<Project, bool> matches, string line)
			{
				Matches = matches;
				this.line = line;
			}

			public override string ToString()
			{
				return line;
			}
		}

		public class Rule
		{
			private readonly string line;
			public readonly Func<Project, bool> Source;
			public readonly Func<Project, bool> Target;
			public readonly bool Allow;

			public Rule(Func<Project, bool> source, Func<Project, bool> target, bool allow, string line)
			{
				Source = source;
				Target = target;
				Allow = allow;
				this.line = line;
			}

			public override string ToString()
			{
				return line;
			}
		}

		public class OutputConfig
		{
			public readonly List<string> Projects = new List<string>();
			public readonly List<string> Groups = new List<string>();
			public readonly List<string> Dependencies = new List<string>();
		}
	}
}
