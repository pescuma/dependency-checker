using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Use: dotnet-dependency-checker <config file>");
				Console.WriteLine();
				return -1;
			}

			Config config;
			try
			{
				config = ConfigParser.Parse(args[0]);
			}
			catch (ConfigParserException e)
			{
				Console.WriteLine("Error parsing config file: " + e.Message);
				Console.WriteLine();
				return -1;
			}

			var warns = new List<string>();

			var graph = ProjectsLoader.LoadGraph(config, warns);

			warns.ForEach(w => Console.WriteLine("\n[warn] " + w));

			Dump(graph.Vertices.Select(p => p.Name), config.Output.Projects);

			var errors = RulesMatcher.Validate(graph, config);

			errors.ForEach(e => Console.WriteLine("\n[ERROR] " + e));

			Console.WriteLine();
			return 0;
		}

		private static void Dump(IEnumerable<string> projs, List<string> filenames)
		{
			if (!filenames.Any())
				return;

			var names = projs.ToList();

			names.Sort();

			filenames.ForEach(f => File.WriteAllLines(f, names));
		}
	}
}
