using System.Linq;
using NUnit.Framework;

namespace org.pescuma.dotnetdependencychecker
{
	[TestFixture]
	public class BuildOrderProcessorTest
	{
		[Test]
		public void TestNoCircularDeps()
		{
			var p1 = TestUtils.ProjWithName("P1");
			var p2 = TestUtils.ProjWithName("P2");
			var dep = TestUtils.Dependency(p1, p2);
			var graph = TestUtils.Graph(p1, p2, dep);

			var result = BuildOrderProcessor.ReplaceCircularDependenciesWithGroup(graph, Enumerable.Empty<CircularDependencyGroup>());

			Assert.AreEqual(2, result.Vertices.Count());
			Assert.AreEqual(1, result.Edges.Count());
		}

		[Test]
		public void TestOneSimpleCircularDep()
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
	}
}
