using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.presenter.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class GroupsCommand : BaseCommand
	{
		public override string Name
		{
			get { return "groups"; }
		}

		protected override void InternalHandle(Output result, string args, DependencyGraph graph)
		{
			var libs = FilterLibs(graph, args);

			ConsoleOutputer.GroupsToConsole(result, libs);
		}
	}
}
