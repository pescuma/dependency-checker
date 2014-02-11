using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker
{
	public class ProjectsLoader
	{
		public static DependencyGraph LoadGraph(Config config, List<string> warns)
		{
			Console.WriteLine("Reading cs projs...");

			var csprojFiles = config.Inputs.SelectMany(folder => Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories));

			var csprojs = csprojFiles.Select(csprojFile => new CsprojReader(csprojFile))
				.ToList();

			var projs = CreateProjects(csprojs)
				.Distinct()
				.ToDictionary(p => p.Name, p => p);

			Console.WriteLine("Creating dependency graph...");

			Func<Project, bool> ignore = p => config.Ignores.Any(i => i.Matches(p));

			var graph = new DependencyGraph();

			projs.Values.Where(p => !ignore(p))
				.ForEach(p => graph.AddVertex(p));

			foreach (var csproj in csprojs)
			{
				var proj = projs[csproj.Name];

				if (ignore(proj))
					continue;

				var deps = CreateDeps(projs, csproj)
					.Where(d => !ignore(d.Source) && !ignore(d.Target))
					.ToList();

				var duplicates = deps.GroupBy(i => i)
					.Where(g => g.Count() > 1)
					.Select(g => g.Key)
					.ToList();

				if (duplicates.Count > 0)
				{
					var warn = new StringBuilder();
					warn.Append(string.Format("The project {0} depends more than once on the following projects:", csproj.Name));
					duplicates.ForEach(d => warn.Append("\n  - " + d.Target.Name));

					warns.Add(warn.ToString());
				}

				deps.Distinct()
					.ForEach(d => graph.AddEdge(d));
			}

			return graph;
		}

		private static IEnumerable<Project> CreateProjects(IList<CsprojReader> csprojs)
		{
			var localProjs = new HashSet<string>(csprojs.Select(p => p.Name));

			foreach (var csproj in csprojs)
			{
				yield return new Project(csproj.Name, csproj.Filename, localProjs.Contains(csproj.Name));

				foreach (var reference in csproj.ProjectReferences)
					yield return new Project(reference.Name, reference.Include, localProjs.Contains(csproj.Name));

				foreach (var reference in csproj.References)
					yield return new Project(reference.Include.Name, reference.HintPath, localProjs.Contains(csproj.Name));
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
	}
}
