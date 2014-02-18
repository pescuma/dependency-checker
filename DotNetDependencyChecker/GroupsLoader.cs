using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker
{
	public class GroupsLoader
	{
		private readonly Dictionary<string, Group> groups = new Dictionary<string, Group>();
		private readonly Config config;
		private readonly DependencyGraph graph;

		public GroupsLoader(Config config, DependencyGraph graph)
		{
			this.config = config;
			this.graph = graph;
		}

		public void FillGroups()
		{
			graph.Vertices.Cast<Assembly>()
				.ForEach(proj => proj.Group = FindGroup(proj));
		}

		private Group FindGroup(Assembly proj)
		{
			var configGroup = config.Groups.FirstOrDefault(g => g.Matches(proj));
			if (configGroup == null)
				return null;

			Group group;
			if (!groups.TryGetValue(configGroup.Name, out group))
			{
				group = new Group(configGroup.Name);
				groups.Add(configGroup.Name, group);
			}

			return group;
		}
	}
}
