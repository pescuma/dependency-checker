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
			var circularDependencies = ComputeCircularDependencies(graph)
				.ToList();

			var nonCircularGraph = ReplaceCircularDependenciesWithGroup(graph, circularDependencies);

			var disjointSet = nonCircularGraph.ToUndirectedGraph()
				.ComputeDisjointSet();

			var result = new BuildScript();

			foreach (var vertices in nonCircularGraph.Vertices.GroupBy(disjointSet.FindSet)
				.Select(s => new HashSet<Dependable>(s)))
			{
				var thread = new BuildThread();
				nonCircularGraph.CreateSubGraph(vertices)
					.TopologicalSort()
					.ForEach(p => Add(thread.Steps, graph, p));

				result.ParallelThreads.Add(thread);
			}

			return result;
		}

		private static void Add(List<BuildStep> steps, DependencyGraph graph, Dependable project)
		{
			if (project is Project)
				AddProject(steps, graph, (Project) project);
			else if (project is CircularDependencyGroup)
				AddCircularDependencyGroup(steps, graph, (CircularDependencyGroup) project);
		}

		private static void AddProject(List<BuildStep> steps, DependencyGraph graph, Project project)
		{
			foreach (var dep in graph.OutEdges(project)
				.Where(e => e.Type == Dependency.Types.DllReference && e.DLLHintPath != null))
			{
				if (dep.Target is Project)
					steps.Add(new CopyProjectOutput((Project) dep.Target, dep.DLLHintPath));
				else
					steps.Add(new MaterializeDll((Assembly) dep.Target, dep.DLLHintPath));
			}

			steps.Add(new BuildProject(project));
		}

		private static void AddCircularDependencyGroup(List<BuildStep> steps, DependencyGraph graph, CircularDependencyGroup project)
		{
			throw new System.NotImplementedException();
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
			List<CircularDependencyGroup> circularDependencies)
		{
			if (!circularDependencies.Any())
				return graph;

			var projToGroup = new Dictionary<Dependable, CircularDependencyGroup>();
			circularDependencies.ForEach(d => d.Projs.ForEach(p => projToGroup.Add(p, d)));

			var vertices = graph.Vertices.Select(v => projToGroup.Get(v) ?? v)
				.Distinct();

			var edges = graph.Edges.Select(e => e.WithSource(projToGroup.Get(e.Source) ?? e.Source)
				.WithTarget(projToGroup.Get(e.Target) ?? e.Target))
				.Where(e => !e.Source.Equals(e.Target))
				.Distinct();

			var result = new DependencyGraph();
			result.AddVertexRange(vertices);
			result.AddEdgeRange(edges.Select(SwapSourceAndTarget));
			return result;
		}

		private static Dependency SwapSourceAndTarget(Dependency dep)
		{
			var source = dep.Source;
			return dep.WithSource(dep.Target)
				.WithTarget(source);
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
}
