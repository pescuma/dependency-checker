using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker
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
			graph.Vertices.ForEach(proj => proj.GroupElement = FindGroupElement(proj));
		}

		private GroupElement FindGroupElement(Library proj)
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

			return new GroupElement(group, configGroup.Location, proj);
		}
	}
}
