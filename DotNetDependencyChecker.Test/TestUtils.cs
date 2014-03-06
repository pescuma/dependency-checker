using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker
{
	internal class TestUtils
	{
		public static Project ProjWithName(string name)
		{
			return new Project(name, "NO LIB NAME", new Guid(), "CSPROJ");
		}

		public static Project[] ProjsWithName(params string[] ps)
		{
			return ps.Select(ProjWithName)
				.ToArray();
		}

		public static Project ProjWithPath(string path)
		{
			return new Project("NO NAME", "NO LIB NAME", new Guid(), path);
		}

		public static Dependency Dependency(Project p1, Project p2)
		{
			return model.Dependency.WithProject(p1, p2, new Location("F", 1));
		}

		public static DependencyGraph Graph(params object[] os)
		{
			var graph = new DependencyGraph();

			graph.AddVertexRange(os.OfType<Library>());
			graph.AddVertexRange(os.OfType<IEnumerable<Library>>()
				.SelectMany(e => e));

			graph.AddEdgeRange(os.OfType<Dependency>());
			graph.AddEdgeRange(os.OfType<IEnumerable<Dependency>>()
				.SelectMany(e => e));

			return graph;
		}
	}
}
