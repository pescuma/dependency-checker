using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dependencychecker.architecture;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.rules;
using org.pescuma.dependencychecker.utils;
using QuickGraph.Algorithms;

namespace org.pescuma.dependencychecker.output.dependencies
{
	public class DotDependenciesOutputer : DependenciesOutputer
	{
		private readonly string file;

		private DependencyGraph graph;
		private ArchitectureGraph architecture;
		private List<OutputEntry> warnings;
		private HashSet<Dependency> wrongDependencies;
		private readonly Dictionary<string, GroupInfo> groupInfos = new Dictionary<string, GroupInfo>();

		public DotDependenciesOutputer(string file)
		{
			this.file = file;
		}

		public void Output(DependencyGraph aGraph, ArchitectureGraph aArchitecture, List<OutputEntry> aWarnings)
		{
			graph = aGraph;
			architecture = aArchitecture;
			warnings = aWarnings;

			wrongDependencies = new HashSet<Dependency>(warnings.OfType<DependencyRuleMatch>()
				.Where(w => !w.Allowed)
				.SelectMany(w => w.Dependencies));

			var result = GenerateDot();

			File.WriteAllText(file, result);
		}

		//private DependencyGraph Sample(DependencyGraph aGraph, List<OutputEntry> aWarnings, int percent)
		//{
		//	var rand = new Random();

		//	var selected = new HashSet<Library>(aWarnings.SelectMany(w => w.Projects)
		//		.Concat(aGraph.Vertices.Where(v => rand.Next(100) < percent)));

		//	var result = new DependencyGraph();
		//	result.AddVertexRange(selected);
		//	result.AddEdgeRange(aGraph.Edges.Where(e => selected.Contains(e.Source) && selected.Contains(e.Target)));
		//	return result;
		//}

		private string GenerateDot()
		{
			var result = new DotStringBuilder("Dependencies");

			result.AppendConfig("concentrate", "true");

			int clusterIndex = 1;

			foreach (var group in graph.Vertices.Where(a => a.GroupElement != null)
				.GroupBy(a => a.GroupElement.Name))
			{
				var groupName = @group.Key;
				var clusterName = "cluster" + clusterIndex++;

				var groupInfo = new GroupInfo(clusterName + "_top", clusterName + "_bottom");
				groupInfos.Add(groupName, groupInfo);

				result.StartSubgraph(clusterName);

				result.AppendConfig("label", groupName);
				result.AppendConfig("color", "lightgray");
				result.AppendConfig("style", "filled");
				result.AppendConfig("fontsize", "20");

				result.AppendSpace();

				AppendGroupNode(result, "min", groupInfo.TopNode);

				result.AppendSpace();

				var projs = new HashSet<Library>(group);

				projs.ForEach(a => AppendProject(result, a));

				result.AppendSpace();

				graph.Edges.Where(e => projs.Contains(e.Source) && projs.Contains(e.Target))
					.ForEach(d => AppendDependency(result, d));

				result.AppendSpace();

				AppendGroupNode(result, "max", groupInfo.BottomNode);

				result.EndSubgraph();

				result.AppendSpace();
			}

			graph.Vertices.Where(a => a.GroupElement == null)
				.ForEach(a => AppendProject(result, a));

			result.AppendSpace();

			graph.Edges.Where(e => AreFromDifferentGroups(e.Source, e.Target))
				.ForEach(d => AppendDependency(result, d));

			result.AppendSpace();

			AddDependenciesBetweenGroups(result);

			return result.ToString();
		}

		private bool AreFromDifferentGroups(Library p1, Library p2)
		{
			if (p1.GroupElement == null || p2.GroupElement == null)
				return true;

			return p1.GroupElement.Name != p2.GroupElement.Name;
		}

		private void AppendProject(DotStringBuilder result, Library library)
		{
			result.AppendNode(library, library.Name, "shape", library is Project ? "box" : "ellipse", "color",
				GetColor(warnings.Where(w => w.Projects.Contains(library))));
		}

		private void AppendDependency(DotStringBuilder result, Dependency dep)
		{
			var invert = wrongDependencies.Contains(dep);

			result.AppendEdge(invert ? dep.Target : dep.Source, invert ? dep.Source : dep.Target, //
				"style", dep.Type == Dependency.Types.LibraryReference ? "dashed" : null, // 
				"color", GetColor(warnings.Where(w => w.Dependencies.Contains(dep))), // 
				"dir", invert ? "back" : null);
		}

		private string GetColor(IEnumerable<OutputEntry> warns)
		{
			var serverities = warns.Select(p => p.Severity)
				.Distinct()
				.ToList();

			if (serverities.Any(s => s == Severity.Error))
				return "red";
			else if (serverities.Any(s => s == Severity.Warning))
				return "darkgoldenrod2";
			else if (serverities.Any(s => s == Severity.Info))
				return "blue";
			else
				return null;
		}

		private static void AppendGroupNode(DotStringBuilder result, string rank, string cluster)
		{
			result.StartGroup()
				.AppendConfig("rank", rank)
				.AppendNode(cluster, "", "style", "invis", "shape", "none", "fixedsize", "true", "width", "0", "height", "0")
				.EndGroup();
		}

		private void AddDependenciesBetweenGroups(DotStringBuilder result)
		{
			var usedGroups = new HashSet<string>(graph.Vertices.Where(v => v.GroupElement != null)
				.Select(g => g.GroupElement.Name));

			if (usedGroups.Count < 2)
				return;

			var usedDeps = architecture.Edges.Where(e => usedGroups.Contains(e.Source) && usedGroups.Contains(e.Target))
				.ToList();

			if (!usedDeps.Any())
				return;

			IDictionary<string, int> components;
			int numConnectedComponents = architecture.StronglyConnectedComponents(out components);
			if (numConnectedComponents < 2)
				return;

			//var circularDeps = components.Select(c => new { Group = c.Key, Index = c.Value })
			//	.Where(e => usedGroups.Contains(e.Group))
			//	.GroupBy(c => c.Index)
			//	.Where(g => g.Count() > 1);

			//foreach (var circ in circularDeps)
			//{
			//	var nodes = circ.ToList();
			//	nodes.Add(nodes.First());

			//	result.Append(prefix)
			//		.Append("{ ")
			//		.Append(string.Join(" -> ", nodes.Select(e => groupInfos[e.Group].TopNode)))
			//		.Append(" [weight=1000];")
			//		.Append("}\n");

			//	result.Append(prefix)
			//		.Append("{ ")
			//		.Append(string.Join(" -> ", nodes.Select(e => groupInfos[e.Group].BottomNode)))
			//		.Append(" [weight=1000];")
			//		.Append("}\n");
			//}

			foreach (var dep in usedDeps)
			{
				if (components[dep.Source] == components[dep.Target])
					continue;

				result.AppendEdge(groupInfos[dep.Source].BottomNode, groupInfos[dep.Target].TopNode, "style", "invis", "weight", "1000");
			}
		}

		private class GroupInfo
		{
			public readonly string TopNode;
			public readonly string BottomNode;

			public GroupInfo(string topNode, string bottomNode)
			{
				TopNode = topNode;
				BottomNode = bottomNode;
			}
		}
	}
}
