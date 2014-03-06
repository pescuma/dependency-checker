using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output.results;
using org.pescuma.dotnetdependencychecker.rules;
using QuickGraph;
using QuickGraph.Algorithms;

namespace org.pescuma.dotnetdependencychecker.output.dependencies
{
	public class DotDependenciesOutputer : DependenciesOutputer
	{
		private const string INDENT = "    ";

		private readonly Config config;
		private readonly string file;
		private readonly bool onlyWithMessages;
		private readonly Dictionary<Library, int> ids = new Dictionary<Library, int>();

		private DependencyGraph fullGraph;
		private DependencyGraph graph;
		private List<OutputEntry> warnings;
		private HashSet<Dependency> wrongDependencies;
		private readonly Dictionary<string, GroupInfo> groupInfos = new Dictionary<string, GroupInfo>();

		public DotDependenciesOutputer(Config config, string file, bool onlyWithMessages)
		{
			this.config = config;
			this.file = file;
			this.onlyWithMessages = onlyWithMessages;
		}

		public void Output(DependencyGraph aGraph, List<OutputEntry> aWarnings)
		{
			fullGraph = aGraph;

			if (onlyWithMessages)
				graph = OnlyWithMessagesDependenciesOutputer.Filter(aGraph, aWarnings);
			else
				graph = aGraph;

			warnings = aWarnings;

			wrongDependencies = new HashSet<Dependency>(warnings.OfType<DependencyRuleMatch>()
				.Where(w => !(w is CircularDependencyRuleMatch))
				.Where(w => !w.Allowed)
				.SelectMany(w => w.Dependencies));

			graph.Vertices.ForEach((proj, i) => ids.Add(proj, i + 1));

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
			var result = new StringBuilder();

			result.Append("digraph Dependencies {\n");
			result.Append(INDENT)
				.Append("concentrate=true;\n");

			int clusterIndex = 1;

			foreach (var group in graph.Vertices.Where(a => a.GroupElement != null)
				.GroupBy(a => a.GroupElement.Name))
			{
				var groupName = @group.Key;
				var clusterName = "cluster" + clusterIndex++;

				var groupInfo = new GroupInfo(clusterName + "_top", clusterName + "_bottom");
				groupInfos.Add(groupName, groupInfo);

				result.Append(INDENT)
					.Append("subgraph ")
					.Append(clusterName)
					.Append("{\n");

				const string subprefix = INDENT + INDENT;

				result.Append(subprefix)
					.Append("label=\"")
					.Append(groupName.Replace('"', ' '))
					.Append("\";\n");
				result.Append(subprefix)
					.Append("color=lightgray;\n");
				result.Append(subprefix)
					.Append("style=filled;\n");
				result.Append(subprefix)
					.Append("fontsize=20;\n");

				result.Append("\n");

				AppendGroupBottom(result, subprefix, groupInfo);

				result.Append("\n");

				var projs = new HashSet<Library>(group);

				projs.ForEach(a => AppendProject(result, subprefix, a));

				result.Append("\n");

				graph.Edges.Where(e => projs.Contains(e.Source) && projs.Contains(e.Target))
					.ForEach(d => AppendDependency(result, subprefix, d));

				result.Append("\n");

				AppendGroupTop(result, subprefix, groupInfo);

				result.Append(INDENT)
					.Append("}\n\n");
			}

			graph.Vertices.Where(a => a.GroupElement == null)
				.ForEach(a => AppendProject(result, INDENT, a));

			result.Append("\n");

			graph.Edges.Where(e => AreFromDifferentGroups(e.Source, e.Target))
				.ForEach(d => AppendDependency(result, INDENT, d));

			result.Append("\n");

			AddDependenciesBetweenGroups(result, INDENT);

			result.Append("}\n");

			return result.ToString();
		}

		private bool AreFromDifferentGroups(Library p1, Library p2)
		{
			if (p1.GroupElement == null || p2.GroupElement == null)
				return true;

			return p1.GroupElement.Name != p2.GroupElement.Name;
		}

		private void AppendProject(StringBuilder result, string prefix, Library library)
		{
			var id = ids[library];
			result.Append(prefix)
				.Append(id)
				.Append(" [");

			if (library is Project)
				result.Append("shape=box");
			else
				result.Append("shape=ellipse");

			result.Append(",label=\"")
				.Append(library.Names.First()
					.Replace('"', ' '))
				.Append("\"");

			var color = GetColor(warnings.Where(w => w.Projects.Contains(library)));
			if (color != null)
				result.Append(",color=\"")
					.Append(color)
					.Append("\"");

			result.Append("];\n");
		}

		private void AppendDependency(StringBuilder result, string prefix, Dependency dep)
		{
			var invert = wrongDependencies.Contains(dep);

			result.Append(prefix);
			result.Append(ids[invert ? dep.Target : dep.Source]);
			result.Append(" -> ");
			result.Append(ids[invert ? dep.Source : dep.Target]);

			var attribs = new List<string>();

			if (dep.Type == Dependency.Types.DllReference)
				attribs.Add("style=dashed");

			var color = GetColor(warnings.Where(w => w.Dependencies.Contains(dep)));
			if (color != null)
				attribs.Add("color=\"" + color + "\"");

			if (invert)
				attribs.Add("dir=back");

			if (attribs.Any())
				result.Append(" [")
					.Append(string.Join(",", attribs))
					.Append("]");

			result.Append(";\n");
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

		private static void AppendGroupTop(StringBuilder result, string subprefix, GroupInfo groupInfo)
		{
			result.Append(subprefix)
				.Append("{ rank=min; ")
				.Append(groupInfo.TopNode)
				.Append(" [style=invis,shape=none,fixedsize=true,width=0,height=0,label=\"\"] }\n");
		}

		private static void AppendGroupBottom(StringBuilder result, string subprefix, GroupInfo groupInfo)
		{
			result.Append(subprefix)
				.Append("{ rank=max; ")
				.Append(groupInfo.BottomNode)
				.Append(" [style=invis,shape=none,fixedsize=true,width=0,height=0,label=\"\"] }\n");
		}

		private void AddDependenciesBetweenGroups(StringBuilder result, string prefix)
		{
			var usedGroups = new HashSet<string>(graph.Vertices.Where(v => v.GroupElement != null)
				.Select(g => g.GroupElement.Name));

			if (usedGroups.Count < 2)
				return;

			var groupsGraph = CreateGroupsGraph(fullGraph.Vertices.Where(v => v.GroupElement != null)
				.ToList());

			if (!groupsGraph.Edges.Any())
				return;

			IDictionary<string, int> components;
			int numConnectedComponents = groupsGraph.StronglyConnectedComponents(out components);
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

			foreach (var dep in groupsGraph.Edges.Where(e => usedGroups.Contains(e.Source) && usedGroups.Contains(e.Target)))
			{
				if (components[dep.Source] == components[dep.Target])
					continue;

				result.Append(prefix)
					.Append(groupInfos[dep.Source].BottomNode)
					.Append(" -> ")
					.Append(groupInfos[dep.Target].TopNode)
					.Append(" [style=invis,weight=1000];\n");
			}
		}

		private BidirectionalGraph<string, GroupDependency> CreateGroupsGraph(List<Library> projs)
		{
			var groupGraph = new BidirectionalGraph<string, GroupDependency>();

			groupGraph.AddVertexRange(projs.Select(p => p.GroupElement.Name)
				.Distinct());

			var deps = new HashSet<GroupDependency>();
			foreach (var p1 in projs)
			{
				foreach (var p2 in projs)
				{
// ReSharper disable once PossibleUnintendedReferenceComparison
					if (p1 == p2)
						continue;

					var match = RulesMatcher.FindMatch(config.Rules, Dependency.WithProject(p1, p2, new Location("a", 1))) as DependencyRuleMatch;
					if (match == null)
						continue;

					if (match.Allowed)
						deps.Add(new GroupDependency(p1.GroupElement.Name, p2.GroupElement.Name));
					else
						deps.Add(new GroupDependency(p2.GroupElement.Name, p1.GroupElement.Name));
				}
			}

			groupGraph.AddEdgeRange(deps);

			return groupGraph;
		}

		private class GroupDependency : Edge<string>
		{
			public GroupDependency(string source, string target)
				: base(source, target)
			{
			}

			private bool Equals(GroupDependency other)
			{
				return string.Equals(Source, other.Source) && string.Equals(Target, other.Target);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
					return false;
				if (ReferenceEquals(this, obj))
					return true;
				if (obj.GetType() != GetType())
					return false;
				return Equals((GroupDependency) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ (Target != null ? Target.GetHashCode() : 0);
				}
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
