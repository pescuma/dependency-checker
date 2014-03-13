using System.Linq;
using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencyconsole.commands
{
	internal class DependenciesOfCommand : BaseReferencesCommand
	{
		public override string Name
		{
			get { return "dependencies of"; }
		}

		protected override void InternalHandle(string args, DependencyGraph graph)
		{
			var inverted = InvertGraph(graph);

			OutputReferences(args, inverted, "dependents");
		}

		private DependencyGraph InvertGraph(DependencyGraph graph)
		{
			var result = new DependencyGraph();
			result.AddVertexRange(graph.Vertices);
			result.AddEdgeRange(graph.Edges.Select(d =>
			{
				var tmp = d.Source;
				return d.WithSource(d.Target)
					.WithTarget(tmp);
			}));
			return result;
		}
	}
}
