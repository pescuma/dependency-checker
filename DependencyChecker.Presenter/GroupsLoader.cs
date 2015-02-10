using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.config;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.presenter.rules;
using org.pescuma.dependencychecker.presenter.utils;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter
{
	public class GroupsLoader
	{
		private readonly Dictionary<string, Group> groups = new Dictionary<string, Group>();
		private readonly Config config;
		private readonly DependencyGraph graph;
		private readonly List<OutputEntry> warnings;
		private readonly HashSet<ConfigLocation> usedGroups = new HashSet<ConfigLocation>();

		public GroupsLoader(Config config, DependencyGraph graph, List<OutputEntry> warnings)
		{
			this.config = config;
			this.graph = graph;
			this.warnings = warnings;
		}

		public void FillGroups()
		{
			graph.Vertices.ForEach(proj => proj.GroupElement = FindGroupElement(proj));

			RuleUtils.ReportUnusedConfig(warnings, "group", config.Groups.Select(i => i.Location), usedGroups);
		}

		private GroupElement FindGroupElement(Library proj)
		{
			var configGroup = config.Groups.FirstOrDefault(g => g.Matches(proj, Matchers.NullReporter));
			if (configGroup == null)
				return null;

			usedGroups.Add(configGroup.Location);

			if (configGroup.Name == null)
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
