using System;
using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.output;
using org.pescuma.dependencychecker.output.architeture;
using org.pescuma.dependencychecker.output.dependencies;
using org.pescuma.dependencychecker.output.results;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.config
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
			public readonly Func<Library, bool> Matches;

			public Group(string name, Func<Library, bool> matches, ConfigLocation location)
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
			public readonly Func<Library, bool> Matches;
			public readonly ConfigLocation Location;

			public Ignore(Func<Library, bool> matches, ConfigLocation location)
			{
				Matches = matches;
				Location = location;
			}

			public override string ToString()
			{
				return Location.LineText;
			}
		}

		public class OutputConfig
		{
			public readonly List<string> Projects = new List<string>();
			public readonly List<string> Groups = new List<string>();
			public readonly List<DependenciesOutputer> Dependencies = new List<DependenciesOutputer>();
			public readonly List<ArchitectureOutputer> Architecture = new List<ArchitectureOutputer>();
			public readonly List<EntryOutputer> Results = new List<EntryOutputer>();
		}

		public class InOutputConfig
		{
// ReSharper disable once MemberHidesStaticFromOuterClass
			public readonly List<Func<OutputEntry, bool>> Ignore = new List<Func<OutputEntry, bool>>();
		}
	}
}
