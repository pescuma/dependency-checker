using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace org.pescuma.dotnetdependencychecker
{
	[TestFixture]
	public class BuildOrderProcessorTest
	{
		[Test]
		public void CurcularDepsDetection_None()
		{
			var ps = TestUtils.ProjsWithName("P0", "P1");
			var dep = TestUtils.Dependency(ps[0], ps[1]);
			var graph = TestUtils.Graph(ps, dep);

			var result = BuildOrderProcessor.ReplaceCircularDependenciesWithGroup(graph, new List<CircularDependencyGroup>());

			Assert.AreEqual(2, result.Vertices.Count());
			Assert.AreEqual(1, result.Edges.Count());
		}

		[Test]
		public void CurcularDepsDetection_OneSimple()
		{
			var ps = TestUtils.ProjsWithName("P0", "P1", "P2", "P3");
			var deps = new[]
			{
				TestUtils.Dependency(ps[0], ps[1]),
				TestUtils.Dependency(ps[1], ps[2]),
				TestUtils.Dependency(ps[2], ps[3]),
				TestUtils.Dependency(ps[2], ps[1]),
			};
			var graph = TestUtils.Graph(ps, deps);

			var cds = BuildOrderProcessor.ComputeCircularDependencies(graph)
				.ToList();

			Assert.AreEqual(1, cds.Count);
			Assert.AreEqual(2, cds[0].Projs.Count);

			var result = BuildOrderProcessor.ReplaceCircularDependenciesWithGroup(graph, cds);

			Assert.AreEqual(3, result.Vertices.Count());
			Assert.AreEqual(2, result.Edges.Count());
		}

		[Test]
		public void TwoProjsNoCurcularDeps()
		{
			var ps = TestUtils.ProjsWithName("P0", "P1");
			var dep = TestUtils.Dependency(ps[0], ps[1]);
			var graph = TestUtils.Graph(ps, dep);

			var result = BuildOrderProcessor.CreateBuildScript(graph);

			Assert.AreEqual(1, result.ParallelThreads.Count);

			var thread = result.ParallelThreads[0];
			Assert.AreEqual(2, thread.Steps.Count);
			Assert.AreEqual(new BuildProject(ps[0]), thread.Steps[0]);
			Assert.AreEqual(new BuildProject(ps[1]), thread.Steps[1]);
		}
	}
}
