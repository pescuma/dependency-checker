using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using org.pescuma.dependencychecker.commands;

namespace org.pescuma.dependencychecker
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Use: dependency-console <dependencies.xml>");
				Console.WriteLine();
				return -1;
			}

			var graph = LoadGraph(args[0]);

			Console.WriteLine("Graph loaded.");

			var commands = new[] { new QuitCommand() };

			try
			{
				while (true)
				{
					Console.WriteLine();
					Console.Write("> ");
					var line = Console.ReadLine();
					if (line == null)
						break;
					line = line.Trim();

					if (line == "help")
					{
						Console.WriteLine("Commands: " + string.Join(", ", commands.Select(c => c.Name)));
						continue;
					}

					var handled = commands.Any(c => c.Handle(line, graph));
					if (!handled)
						Console.WriteLine("Unknown: " + line);
				}
			}
			catch (QuitException)
			{
			}

			return 0;
		}

		private static DependencyGraph LoadGraph(string filename)
		{
			var doc = XDocument.Load(filename);

			var libs = new Dictionary<string, Library>();
			foreach (var el in doc.Descendants("Element"))
			{
				var lib = new Library();

				lib.Type = el.Attribute("Type")
					.Value;
				el.Descendants("Name")
					.ForEach(d => lib.Names.Add(d.Value));
				el.Descendants("Language")
					.ForEach(d => lib.Languages.Add(d.Value));
				el.Descendants("Path")
					.ForEach(d => lib.Paths.Add(d.Value));

				var group = el.Element("Group");
				if (group != null)
					lib.Group = group.Attribute("Name")
						.Value;

				libs.Add(lib.Names.First(), lib);
			}

			var deps = new List<Dependency>();
			foreach (var el in doc.Descendants("Dependency"))
			{
				var source = el.Attribute("Source")
					.Value;
				var target = el.Attribute("Target")
					.Value;
				var type = el.Attribute("Type")
					.Value;

				var path = el.Element("ReferencedPath");
				string referencedPath = (path != null ? path.Value : null);

				deps.Add(new Dependency(libs[source], libs[target], type, referencedPath));
			}

			var graph = new DependencyGraph();
			graph.AddVertexRange(libs.Values);
			graph.AddEdgeRange(deps);
			return graph;
		}
	}
}
