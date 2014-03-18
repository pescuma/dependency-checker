using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.utils;
using org.pescuma.dependencychecker.utils;
using QuickGraph.Algorithms.Search;

namespace org.pescuma.dependencyconsole.commands
{
	internal abstract class BaseReferencesCommand : BaseCommand
	{
		protected void OutputReferences(Output result, string args, DependencyGraph graph, string type)
		{
			var libs = FilterLibs(graph, args);

			if (!libs.Any())
				result.AppendLine("No libraries found");
			else
				libs.SortBy(Library.NaturalOrdering)
					.ForEach(l => OutputReferences(result, graph, l, type));
		}

		private void OutputReferences(Output result, DependencyGraph graph, Library lib, string type)
		{
			result.AppendLine(GetName(lib) + ":");

			var directDeps = new HashSet<Library>(graph.OutEdges(lib)
				.Select(d => d.Target));

			result.IncreaseIndent();
			result.AppendLine("Direct " + type + ":");

			result.IncreaseIndent();
			Output(result, directDeps);
			result.DecreaseIndent();

			result.DecreaseIndent();

			if (directDeps.Any())
			{
				var indirectDeps = ComputeIndirectDeps(graph, lib)
					.Where(d => !directDeps.Contains(d))
// ReSharper disable once PossibleUnintendedReferenceComparison
					.Where(d => d != lib);

				result.IncreaseIndent();
				result.AppendLine("Indirect " + type + ":");

				result.IncreaseIndent();
				Output(result, indirectDeps);
				result.DecreaseIndent();

				result.DecreaseIndent();
			}

			result.AppendLine();
		}

		private void Output(Output result, IEnumerable<Library> depsList)
		{
			var deps = depsList as IList<Library> ?? depsList.ToList();

			if (!deps.Any())
				result.AppendLine("none found");
			else
				deps.SortBy(Library.NaturalOrdering)
					.ForEach(l => result.AppendLine(GetName(l)));
		}

		private static List<Library> ComputeIndirectDeps(DependencyGraph graph, Library lib)
		{
			var indirectDeps = new List<Library>();

			var dfs = new DepthFirstSearchAlgorithm<Library, Dependency>(graph);
			dfs.SetRootVertex(lib);
			dfs.DiscoverVertex += indirectDeps.Add;
			dfs.Compute();

			return indirectDeps;
		}
	}
}
