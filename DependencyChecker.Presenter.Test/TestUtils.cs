using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencychecker.presenter
{
	internal class TestUtils
	{
		public static Project ProjWithName(string name)
		{
			return new Project(name, "NO LIB NAME", new Guid(), "PROJ.PATH", null);
		}

		public static Project[] ProjsWithName(params string[] ps)
		{
			return ps.Select(ProjWithName)
				.ToArray();
		}

		public static Project ProjWithPath(string path)
		{
			return new Project("NO NAME", "NO LIB NAME", new Guid(), path, null);
		}

		public static Project LocalProj()
		{
			return new Project("NO NAME", "NO LIB NAME", new Guid(), "PROJ.PATH", null) { IsLocal = true };
		}

		public static Project NonLocalProj()
		{
			return new Project("NO NAME", "NO LIB NAME", new Guid(), "PROJ.PATH", null) { IsLocal = false };
		}

		public static Dependency ProjectDependency(string p1, string p2)
		{
			return Dependency.WithProject(ProjWithName(p1), ProjWithName(p2), new Location("F", 1));
		}

		public static Dependency LibraryDependency(string p1, string p2, string path)
		{
			return Dependency.WithLibrary(ProjWithName(p1), ProjWithName(p2), new Location("F", 1), path);
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
