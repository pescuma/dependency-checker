using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.output;

namespace org.pescuma.dotnetdependencychecker.input
{
	public class ProjectsLoader
	{
		public static DependencyGraph LoadGraph(Config config, List<OutputEntry> warnings)
		{
			ProjectLoader[] loaders = { new CsprojectsLoader() };

			var builder = new DependencyGraphBuilder(config, warnings);

			loaders.ForEach(l => l.LoadProjects(config.Inputs, builder, warnings));

			return builder.Build();
		}
	}
}
