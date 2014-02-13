using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker
{
	public class ProjectsLoader
	{
		public static DependencyGraph LoadGraph(Config config, List<RuleMatch> warns)
		{
			Console.WriteLine("Reading cs projs...");

			var csprojFiles = config.Inputs.SelectMany(folder => Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories))
				.Select(Path.GetFullPath)
				.Distinct();

			Func<Project, bool> ignore = p => config.Ignores.Any(i => i.Matches(p));

			var csprojs = csprojFiles.Select(csprojFile => new CsprojReader(csprojFile))
				.Where(csproj => !ignore(ToProject(csproj)))
				.ToList();

			var matches = TestProjectsWithSame(csprojs, p => p.Name, "name");
			if (matches.Any())
			{
				var msg = new StringBuilder();
				msg.Append("You have multiple projects with the same name. You have to fix it or ignore some of those to be able to process them.\n"
				           + "The affected projects are:");
				matches.ForEach(m => msg.Append("\n\n")
					.Append(m.Messsage));
				throw new ConfigException(msg.ToString());
			}

			warns.AddRange(TestProjectsWithSame(csprojs, p => p.ProjectGuid.ToString(), "GUI"));

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
					duplicates.Sort(Dependency.NaturalOrdering);

					var warn = new StringBuilder();
					warn.Append("The project ")
						.Append(csproj.Name)
						.Append(" depends more than once on the following projects:");
					duplicates.ForEach(d => warn.Append("\n  - " + d.Target.Name));

					warns.Add(new RuleMatch(false, Severity.Info, warn.ToString(), null, new List<Project> { proj }, duplicates));
				}

				deps.ForEach(d => graph.AddEdge(d));
			}

			return graph;
		}

		private static List<RuleMatch> TestProjectsWithSame(List<CsprojReader> csprojs, Func<CsprojReader, string> id, string name)
		{
			var csprojsWithSameName = csprojs.GroupBy(id)
				.Where(g => g.Count() > 1)
				.ToList();

			if (!csprojsWithSameName.Any())
				return null;

			var result = new List<RuleMatch>();

			foreach (var g in csprojsWithSameName)
			{
				var err = new StringBuilder();
				err.Append("You have multiple projects with the ")
					.Append(name)
					.Append(" ")
					.Append(g.Key)
					.Append(":");
				g.ForEach(p => err.Append("\n  - ")
					.Append(p.Filename));

				var projs = g.Select(ToProject)
					.ToList();
				projs.Sort(Project.NaturalOrdering);

				result.Add(new RuleMatch(false, Severity.Warn, err.ToString(), null, projs, null));
			}

			return result;
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
