using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.utils;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class LibsCommand : BaseCommand
	{
		public override string Name
		{
			get { return "libs"; }
		}

		protected override void InternalHandle(Output result, string args, DependencyGraph graph)
		{
			var libs = FilterLibs(graph, args);

			if (!libs.Any())
				result.AppendLine("No libraries found");
			else
				libs.SortBy(Library.NaturalOrdering)
					.ForEach(l => result.AppendLine(GetName(l)));
		}
	}
}
