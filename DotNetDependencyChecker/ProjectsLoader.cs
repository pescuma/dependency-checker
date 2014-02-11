using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker
{
	public class ProjectsLoader
	{
		public static DependencyGraph LoadGraph(Config config)
		{
			Console.WriteLine("Reading cs projs...");

			var csprojFiles = config.Inputs.SelectMany(folder => Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories));

			var csprojs = csprojFiles.Select(csprojFile => new CsprojReader(csprojFile))
				.ToList();

			var localProjs = new HashSet<string>(csprojs.Select(p => p.Name));

			Dump(localProjs, config.Output.LocalProjects);

			var projs = CreateProjects(csprojs)
				.Distinct()
				.ToDictionary(p => p.Name, p => p);

			Dump(projs.Values.Select(p => p.Name), config.Output.AllProjects);

			Console.WriteLine("Creating dependency graph...");

			Func<string, bool> ignore = p => false;

			var graph = new DependencyGraph();

			projs.Values.Where(p2 => !ignore(p2.Name))
				.ForEach(p3 => graph.AddVertex(p3));

			foreach (var csproj in csprojs)
			{
				if (ignore(csproj.Name))
					continue;

				var deps = CreateDeps(projs, csproj)
					.Where(d => !ignore(d.Source.Name) && !ignore(d.Target.Name))
					.ToList();

				var duplicates = deps.GroupBy(i => i)
					.Where(g => g.Count() > 1)
					.Select(g => g.Key)
					.ToList();

				if (duplicates.Count > 0)
				{
					Console.WriteLine("WARN: The projext {0} depends more than once on the following projects:", csproj.Name);
					duplicates.ForEach(d => Console.WriteLine("       - " + d.Target.Name));
				}

				deps.Distinct()
					.ForEach(d => graph.AddEdge(d));
			}

			return graph;
		}

		private static IEnumerable<Project> CreateProjects(IEnumerable<CsprojReader> csprojs)
		{
			foreach (var csproj in csprojs)
			{
				yield return new Project(csproj.Name, csproj.Filename);

				foreach (var reference in csproj.ProjectReferences)
					yield return new Project(reference.Name, reference.Include);

				foreach (var reference in csproj.References)
					yield return new Project(reference.Include.Name, reference.HintPath);
			}
		}

		private static IEnumerable<Dependency> CreateDeps(Dictionary<string, Project> projs, CsprojReader csproj)
		{
			var proj = projs[csproj.Name];

			foreach (var reference in csproj.ProjectReferences)
				yield return new Dependency(proj, projs[reference.Name], Dependency.Types.ProjectReference);

			foreach (var reference in csproj.References)
				yield return new Dependency(proj, projs[reference.Include.Name], Dependency.Types.DllReference);
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
