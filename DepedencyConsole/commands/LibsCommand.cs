using System;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class LibsCommand : BaseCommand
	{
		public override string Name
		{
			get { return "libs"; }
		}

		protected override void InternalHandle(string args, DependencyGraph graph)
		{
			var libs = FilterLibs(graph, args);

			if (!libs.Any())
				Console.WriteLine("No libraries found");
			else
				libs.SortBy(Library.NaturalOrdering)
					.ForEach(l => Console.WriteLine(GetName(l)));
		}
	}
}
