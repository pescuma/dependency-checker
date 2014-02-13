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

			var csprojFiles = config.Inputs.SelectMany(folder => Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories))
				.Select(Path.GetFullPath)
				.Distinct();

			Func<Project, bool> ignore = p => config.Ignores.Any(i => i.Matches(p));

			var csprojs = csprojFiles.Select(csprojFile => new CsprojReader(csprojFile))
				.Where(csproj => !ignore(ToProject(csproj)))
				.ToList();

			var msg = TestProjectsWithSame(csprojs, p => p.Name, "name");
			if (msg != null)
				throw new ConfigException(msg);

			msg = TestProjectsWithSame(csprojs, p => p.ProjectGuid.ToString(), "GUI");
			if (msg != null)
				warns.Add(msg);

			var projs = CreateProjects(csprojs);

			Console.WriteLine("Creating dependency graph...");

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

				deps.ForEach(d => graph.AddEdge(d));
			}

			return graph;
		}

		private static string TestProjectsWithSame(List<CsprojReader> csprojs, Func<CsprojReader, string> id, string name)
		{
			var csprojsWithSameName = csprojs.GroupBy(id)
				.Where(g => g.Count() > 1)
				.ToList();

			if (!csprojsWithSameName.Any())
				return null;

			var err = new StringBuilder();
			err.Append("You have multiple projects with the same ")
				.Append(name)
				.Append(":");
			foreach (var g in csprojsWithSameName)
			{
				err.Append("\n")
					.Append(g.Key);
				g.ForEach(p => err.Append("\n  - ")
					.Append(p.Filename));
			}
			return err.ToString();
		}

		private static Dictionary<string, Project> CreateProjects(IList<CsprojReader> csprojs)
		{
			var projs = new Dictionary<string, Project>();

			foreach (var csproj in csprojs)
				projs.Add(csproj.Name, ToProject(csproj));

			foreach (var csproj in csprojs)
			{
				foreach (var reference in csproj.ProjectReferences)
				{
					var name = reference.Name;

					Project proj;
					if (!projs.TryGetValue(name, out proj))
					{
						proj = new Project(name, null);
						projs.Add(name, proj);
					}

					proj.Paths.Add(reference.Include);
				}

				foreach (var reference in csproj.References)
				{
					var name = reference.Include.Name;

					Project proj;
					if (!projs.TryGetValue(name, out proj))
					{
						proj = new Project(name, null);
						projs.Add(name, proj);
					}

					if (!string.IsNullOrWhiteSpace(reference.HintPath))
						proj.Paths.Add(reference.HintPath);
				}
			}

			return projs;
		}

		private static Project ToProject(CsprojReader csproj)
		{
			return new Project(csproj.Name, csproj.Filename);
		}

		private static IEnumerable<Dependency> CreateDeps(Dictionary<string, Project> projs, CsprojReader csproj)
		{
			var proj = projs[csproj.Name];

			foreach (var reference in csproj.ProjectReferences)
				yield return
					new Dependency(proj, projs[reference.Name], Dependency.Types.ProjectReference, new Location(csproj.Filename, reference.LineNumber));

			foreach (var reference in csproj.References)
				yield return
					new Dependency(proj, projs[reference.Include.Name], Dependency.Types.DllReference, new Location(csproj.Filename, reference.LineNumber))
					;
		}
	}
}
