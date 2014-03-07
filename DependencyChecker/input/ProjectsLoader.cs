﻿using System.Collections.Generic;
using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.input.loaders;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.output;

namespace org.pescuma.dependencychecker.input
{
	public class ProjectsLoader
	{
		public static DependencyGraph LoadGraph(Config config, List<OutputEntry> warnings)
		{
			ProjectLoader[] loaders = { new CsprojectsLoader(), new EclipseProjectsLoader() };

			var builder = new DependencyGraphBuilder(config, warnings);

			loaders.ForEach(l => l.LoadProjects(config.Inputs, builder, warnings));

			return builder.Build();
		}
	}
}
