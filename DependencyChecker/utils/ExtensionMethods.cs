using System.Collections.Generic;
using QuickGraph;

namespace org.pescuma.dependencychecker.utils
{
	internal static class ExtensionMethods
	{
		public static TV Get<TK, TV>(this Dictionary<TK, TV> dict, TK key) where TV : class
		{
			TV result;
			if (dict.TryGetValue(key, out result))
				return result;
			else
				return null;
		}

		public static IUndirectedGraph<TVertex, TEdge> ToUndirectedGraph<TVertex, TEdge>(this IBidirectionalGraph<TVertex, TEdge> graph)
			where TEdge : Edge<TVertex>
		{
			var result = new UndirectedGraph<TVertex, TEdge>(true);
			result.AddVertexRange(graph.Vertices);
			result.AddEdgeRange(graph.Edges);
			return result;
		}

		public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> toAdd)
		{
			foreach (var e in toAdd)
				set.Add(e);
		}
	}
}
