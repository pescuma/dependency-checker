using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.utils;
using QuickGraph.Algorithms;

namespace org.pescuma.dotnetdependencychecker
{
	public class BuildOrderProcessor
	{
		public static BuildScript CreateBuildScript(DependencyGraph graph)
		{
			var circularDependencies = ComputeCircularDependencies(graph);

			var nonCircularGraph = ReplaceCircularDependenciesWithGroup(graph, circularDependencies);

			return null;
		}

		public static IEnumerable<CircularDependencyGroup> ComputeCircularDependencies(DependencyGraph graph)
		{
			IDictionary<Dependable, int> components;
			graph.StronglyConnectedComponents(out components);

			return components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1)
				.Select(g => new CircularDependencyGroup(g.Select(e => e.Proj)));
		}

		public static DependencyGraph ReplaceCircularDependenciesWithGroup(DependencyGraph graph,
			IEnumerable<CircularDependencyGroup> circularDependencies)
		{
			var projToGroup = new Dictionary<Dependable, CircularDependencyGroup>();
			circularDependencies.ForEach(d => d.Projs.ForEach(p => projToGroup.Add(p, d)));

			if (!projToGroup.Any())
				return graph;

			var vertices = graph.Vertices.Select(v => projToGroup.Get(v) ?? v)
				.Distinct();

			var edges = graph.Edges.Select(e => e.WithSource(projToGroup.Get(e.Source) ?? e.Source)
				.WithTarget(projToGroup.Get(e.Target) ?? e.Target))
				.Where(e => !e.Source.Equals(e.Target))
				.Distinct();

			var result = new DependencyGraph();
			result.AddVertexRange(vertices);
			result.AddEdgeRange(edges);
			return result;
		}
	}

	public class CircularDependencyGroup : Dependable
	{
		public readonly HashSet<Dependable> Projs;

		public CircularDependencyGroup(IEnumerable<Dependable> projs)
		{
			Projs = new HashSet<Dependable>(projs);
		}

		public IEnumerable<string> Names
		{
			get { return Projs.SelectMany(p => p.Names); }
		}

		public IEnumerable<string> Paths
		{
			get { return Projs.SelectMany(p => p.Paths); }
		}

		public override string ToString()
		{
			return "CircularDependencyGroup[" + string.Join(",", Projs.Select(p => p.Names.First())) + "]";
		}
	}

	public class BuildScript
	{
	}
}
