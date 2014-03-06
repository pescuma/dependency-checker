using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.output;

namespace org.pescuma.dotnetdependencychecker.input
{
	public interface ProjectLoader
	{
		void LoadProjects(List<string> paths, DependencyGraphBuilder builder, List<OutputEntry> warnings);
	}
}
