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
			ProjectLoader[] loaders = { new VsProjectsLoader(), new EclipseProjectsLoader() };

			var builder = new DependencyGraphBuilder(config, warnings);

			loaders.ForEach(l => l.LoadProjects(config.Inputs, builder, warnings));

			var graph = builder.Build();

			return graph;
		}
	}
}
