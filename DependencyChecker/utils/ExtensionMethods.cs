using QuickGraph;

namespace org.pescuma.dependencychecker.utils
{
	internal static class ExtensionMethods
	{
		public static IUndirectedGraph<TVertex, TEdge> ToUndirectedGraph<TVertex, TEdge>(this IBidirectionalGraph<TVertex, TEdge> graph)
			where TEdge : Edge<TVertex>
		{
			var result = new UndirectedGraph<TVertex, TEdge>(true);
			result.AddVertexRange(graph.Vertices);
			result.AddEdgeRange(graph.Edges);
			return result;
		}
	}
}
