using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencyconsole.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class DependentsOfCommand : BaseReferencesCommand
	{
		public override string Name
		{
			get { return "dependents of"; }
		}

		protected override void InternalHandle(Output result, string args, DependencyGraph graph)
		{
			if (args == "")
			{
				result.AppendLine("You need to specify a filter for the libraries");
				return;
			}

			var inverted = InvertGraph(graph);

			OutputReferences(result, args, inverted, "dependents");
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
