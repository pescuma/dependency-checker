using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;
using QuickGraph.Algorithms.Search;

namespace org.pescuma.dependencyconsole.commands
{
	internal abstract class BaseReferencesCommand : BaseCommand
	{
		protected void OutputReferences(string args, DependencyGraph graph, string type)
		{
			var libs = FilterLibs(graph, args);

			if (!libs.Any())
				Console.WriteLine("No libraries found");
			else
				libs.SortBy(Library.NaturalOrdering)
					.ForEach(l => OutputReferences(graph, l, type));
		}

		private void OutputReferences(DependencyGraph graph, Library lib, string type)
		{
			Console.WriteLine(GetName(lib) + ":");

			var directDeps = new HashSet<Library>(graph.OutEdges(lib)
				.Select(d => d.Target));

			Console.WriteLine(PREFIX + "Direct " + type + ":");

			Output(directDeps);

			if (!directDeps.Any())
				return;

			Console.WriteLine(PREFIX + "Indirect " + type + ":");

			var indirectDeps = ComputeIndirectDeps(graph, lib)
				.Where(d => !directDeps.Contains(d))
// ReSharper disable once PossibleUnintendedReferenceComparison
				.Where(d => d != lib);

			Output(indirectDeps);
		}

		private void Output(IEnumerable<Library> depsList)
		{
			var deps = depsList as IList<Library> ?? depsList.ToList();

			if (!deps.Any())
				Console.WriteLine(PREFIX + PREFIX + "none found");
			else
				deps.SortBy(Library.NaturalOrdering)
					.ForEach(l => Console.WriteLine(PREFIX + PREFIX + GetName(l)));
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
