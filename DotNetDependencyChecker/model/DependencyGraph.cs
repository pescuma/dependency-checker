﻿using System.Collections.Generic;
using System.Linq;
using QuickGraph;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class DependencyGraph : BidirectionalGraph<Assembly, Dependency>
	{
		public DependencyGraph CreateSubGraph(HashSet<Assembly> vertices)
		{
			var result = new DependencyGraph();
			result.AddVertexRange(vertices);
			result.AddEdgeRange(Edges.Where(e => vertices.Contains(e.Source) && vertices.Contains(e.Target)));
			return result;
		}
	}
}
