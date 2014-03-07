using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.output;

namespace org.pescuma.dependencychecker.architecture
{
	public class ArchitectureLoader
	{
		public static ArchitectureGraph Load(Config config, DependencyGraph graph)
		{
			var libs = graph.Vertices.Where(v => v.GroupElement != null)
				.ToList();

			var allowed = new HashSet<GroupDependency>();
			var notAllowed = new HashSet<GroupDependency>();

			foreach (var p1 in libs)
			{
				foreach (var p2 in libs)
				{
					if (p1.GroupElement.Name == p2.GroupElement.Name)
						continue;

					var dep = new GroupDependency(p1.GroupElement.Name, p2.GroupElement.Name);
					if (allowed.Contains(dep) && notAllowed.Contains(dep))
						continue;

					var match = RulesMatcher.FindMatch(config.Rules, Dependency.WithProject(p1, p2, new Location("a", 1))) as DependencyRuleMatch;
					if (match == null)
						continue;

					if (match.Allowed)
						allowed.Add(dep);
					else
						notAllowed.Add(dep);
				}
			}

			var groups = libs.Select(p => p.GroupElement.Name)
				.Distinct()
				.ToList();

			var deps = new HashSet<GroupDependency>();
			foreach (var g1 in groups)
			{
				foreach (var g2 in groups)
				{
					if (g1 == g2)
						continue;

					var dep = new GroupDependency(g1, g2);
					var isAllowed = allowed.Contains(dep);
					var isNotAllowed = notAllowed.Contains(dep);

					if (isAllowed && isNotAllowed)
						dep = new GroupDependency(g1, g2, true);
					else if (!isAllowed && !isNotAllowed)
						dep = new GroupDependency(g1, g2, false, true);
					else if (isNotAllowed)
						dep = null;

					if (dep != null)
						deps.Add(dep);
				}
			}

			var groupGraph = new ArchitectureGraph();
			groupGraph.AddVertexRange(groups);
			groupGraph.AddEdgeRange(deps);
			return groupGraph;
		}
	}
}
