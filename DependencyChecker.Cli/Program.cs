﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter;
using org.pescuma.dependencychecker.presenter.architecture;
using org.pescuma.dependencychecker.presenter.config;
using org.pescuma.dependencychecker.presenter.input;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.presenter.rules;
using org.pescuma.dependencychecker.presenter.utils;

namespace org.pescuma.dependencychecker.cli
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Use: dependency-checker <config file>");
				Console.WriteLine();
				return -1;
			}

			try
			{
				var warnings = new List<OutputEntry>();

				Config config = new ConfigParser().Parse(args[0]);

				DependencyGraph graph = ProjectsLoader.LoadGraph(config, warnings);

				new GroupsLoader(config, graph, warnings).FillGroups();

				ArchitectureGraph architecture = ArchitectureLoader.Load(config, graph);

				warnings.AddRange(RulesMatcher.Match(graph, config.Rules));

				warnings = warnings.Where(e => !(e is DependencyRuleMatch) || !((DependencyRuleMatch) e).Allowed)
					.Where(w => !config.InOutput.Ignore.Any(f => f(w)))
					.ToList();

				DumpProjects(graph.Vertices, config.Output.Projects);
				DumpGroups(graph.Vertices, config.Output.Groups);
				config.Output.Results.ForEach(o => o.Output(warnings));
				config.Output.Dependencies.ForEach(o => o.Output(graph, architecture, warnings));
				config.Output.Architecture.ForEach(o => o.Output(architecture, warnings));

				DumpNumberOfErrorsFound(warnings);
				Console.WriteLine();
				return warnings.Count(w => w.Severity == Severity.Error);
			}
			catch (ConfigParserException e)
			{
				Console.WriteLine("Error parsing config file: " + e.Message);
				Console.WriteLine();
				return -1;
			}
			catch (ConfigException e)
			{
				Console.WriteLine("Error: " + e.Message);
				Console.WriteLine();
				return -1;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Console.WriteLine();
				return -1;
			}
		}

		private static void DumpNumberOfErrorsFound(List<OutputEntry> warnings)
		{
			if (!warnings.Any())
			{
				Console.WriteLine("No errors found");
				return;
			}

			var gs = warnings.GroupBy(w => w.Severity)
				.Select(e => new { Severity = e.Key, Count = e.Count() })
				.ToList();
			gs.Sort((s1, s2) => (int) s2.Severity - (int) s1.Severity);

			Console.WriteLine("Found " + string.Join(", ", gs.Select(e => e.Count + " " + e.Severity.ToString()
				.ToLower() + "(s)")));
		}

		private static void DumpProjects(IEnumerable<Library> projects, List<string> filenames)
		{
			if (!filenames.Any())
				return;

			List<Library> projs = projects.ToList();
			projs.Sort(Library.NaturalOrdering);

			List<string> names = projs.Select(ConsoleOutputer.ProjectGlance)
				.ToList();

			foreach (string filename in filenames)
			{
// ReSharper disable once AssignNullToNotNullAttribute
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
				File.WriteAllLines(filename, names);
			}
		}

		private static void DumpGroups(IEnumerable<Library> projects, List<string> filenames)
		{
			if (!filenames.Any())
				return;

			var result = new Output("  - ");
			ConsoleOutputer.GroupsToConsole(result, projects);
			string text = result.ToString();

			foreach (string filename in filenames)
			{
// ReSharper disable once AssignNullToNotNullAttribute
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
				File.WriteAllText(filename, text);
			}
		}
	}
}
