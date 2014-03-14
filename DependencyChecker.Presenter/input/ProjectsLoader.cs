using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.config;
using org.pescuma.dependencychecker.presenter.input.loaders;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.input
{
	public class ProjectsLoader
	{
		public static DependencyGraph LoadGraph(Config config, List<OutputEntry> warnings)
		{
			ProjectLoader[] loaders = { new VsprojectsLoader(), new EclipseProjectsLoader() };

			var builder = new DependencyGraphBuilder(config, warnings);

			loaders.ForEach(l => l.LoadProjects(config.Inputs, builder, warnings));

			var graph = builder.Build();

			graph.Vertices.ForEach(p => p.IsLocal = IsLocal(config, p));

			return graph;
		}

		private static bool IsLocal(Config config, Library lib)
		{
			if (lib is Project)
				return config.Inputs.Any(input => PathUtils.PathMatches(((Project) lib).ProjectPath, input));
			else
				return config.Inputs.Any(input => lib.Paths.Any(p => PathUtils.PathMatches(p, input)));
		}
	}
}
