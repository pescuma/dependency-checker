using System;
using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;
using org.pescuma.dotnetdependencychecker.output.dependencies;
using org.pescuma.dotnetdependencychecker.output.results;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.config
{
	public class Config
	{
		public readonly List<string> Inputs = new List<string>();
		public readonly List<Group> Groups = new List<Group>();
		public readonly List<Ignore> Ignores = new List<Ignore>();
		public readonly List<Rule> Rules = new List<Rule>();
		public readonly OutputConfig Output = new OutputConfig();
		public readonly InOutputConfig InOutput = new InOutputConfig();

		public class Group
		{
			public readonly string Name;
			public readonly ConfigLocation Location;
			public readonly Func<Assembly, bool> Matches;

			public Group(string name, Func<Assembly, bool> matches, ConfigLocation location)
			{
				Name = name;
				Matches = matches;
				Location = location;
			}

			public override string ToString()
			{
				return Location.LineText;
			}
		}

		public class Ignore
		{
			public readonly Func<Assembly, bool> Matches;
			private readonly ConfigLocation location;

			public Ignore(Func<Assembly, bool> matches, ConfigLocation location)
			{
				Matches = matches;
				this.location = location;
			}

			public override string ToString()
			{
				return location.LineText;
			}
		}

		public class OutputConfig
		{
			public readonly List<string> Projects = new List<string>();
			public readonly List<string> Groups = new List<string>();
			public readonly List<DependenciesOutputer> Dependencies = new List<DependenciesOutputer>();
			public readonly List<EntryOutputer> Results = new List<EntryOutputer>();
		}

		public class InOutputConfig
		{
// ReSharper disable once MemberHidesStaticFromOuterClass
			public readonly List<Func<OutputEntry, bool>> Ignore = new List<Func<OutputEntry, bool>>();
		}
	}
}
