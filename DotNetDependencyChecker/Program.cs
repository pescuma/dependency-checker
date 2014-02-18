using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

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

			try
			{
				var warnings = new List<RuleMatch>();

				var config = new ConfigParser().Parse(args[0]);

				var graph = new ProjectsLoader(config, warnings).LoadGraph();

				Dump(graph.Vertices.Select(p => p.Names.First()), config.Output.Projects);

				warnings.AddRange(RulesMatcher.Match(graph, config)
					.Where(e => !e.Allowed));

				if (warnings.Any())
					warnings.ForEach(e => Console.WriteLine("\n[{0}] {1}", e.Severity.ToString()
						.ToLower(), ToConsole(e.Messsage)));
				else
					Console.WriteLine("No errors found");

				Console.WriteLine();
				return 0;
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
		}

		private static string ToConsole(OutputMessage messsage)
		{
			return string.Join("", messsage.Elements.Select(e =>
			{
				if (e.Text != null)
				{
					return e.Text;
				}
				else if (e.Project != null)
				{
					var proj = e.Project;
					switch (e.ProjInfo)
					{
						case OutputMessage.ProjInfo.Name:
						{
							if (proj.Paths.Any())
								return string.Format("{0} ({1})", proj.Names.First(), proj.Paths.First());
							else
								return proj.Names.First();
						}
						case OutputMessage.ProjInfo.Path:
						{
							if (proj.Paths.Any())
								return proj.Paths.First();
							else
								return proj.Names.First();
						}
						default:
							throw new InvalidDataException();
					}
				}
				else if (e.Dependendcy != null)
				{
					var dep = e.Dependendcy;
					switch (e.DepInfo)
					{
						case OutputMessage.DepInfo.Type:
						{
							switch (dep.Type)
							{
								case Dependency.Types.DllReference:
									return "DLL reference";
								case Dependency.Types.ProjectReference:
									return "project reference";
								default:
									throw new InvalidDataException();
							}
						}
						case OutputMessage.DepInfo.Line:
						{
							return "line " + dep.Location.Line;
						}
						default:
							throw new InvalidDataException();
					}
				}
				else
					throw new InvalidDataException();
			}));
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
