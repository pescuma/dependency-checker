using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter;
using org.pescuma.dependencychecker.presenter.config;
using org.pescuma.dependencychecker.presenter.output.results;
using org.pescuma.dependencychecker.presenter.utils;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class RuleCommand : BaseConfigCommand
	{
		public override string Name
		{
			get { return "rule:"; }
		}

		protected override void InternalHandle(Output result, Config config, DependencyGraph graph)
		{
			var matches = RulesMatcher.Match(graph, config.Rules);

			if (!matches.Any())
			{
				result.AppendLine("No matches found");
				return;
			}

			foreach (var m in matches)
			{
				ConsoleEntryOutputer.ToConsole(m, false)
					.Split('\n')
					.ForEach(l => result.AppendLine(l));
				result.AppendLine();
			}
		}
	}
}
