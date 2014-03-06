using System.Collections.Generic;
using org.pescuma.dependencychecker.output;

namespace org.pescuma.dependencychecker.input
{
	public interface ProjectLoader
	{
		void LoadProjects(List<string> paths, DependencyGraphBuilder builder, List<OutputEntry> warnings);
	}
}
