using System.Collections.Generic;
using org.pescuma.dependencychecker.presenter.output;

namespace org.pescuma.dependencychecker.presenter.input.loaders
{
	public interface ProjectLoader
	{
		void LoadProjects(List<string> paths, DependencyGraphBuilder builder, List<OutputEntry> warnings);
	}
}
