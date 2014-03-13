using System;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class LibsCommand : Command
	{
		public string Name
		{
			get { return "libs"; }
		}

		public bool Handle(string line, DependencyGraph graph)
		{
			if (!Name.Equals(line))
				return false;

			Console.WriteLine("All libraries:");
			graph.Vertices.Sort(Library.NaturalOrdering)
				.ForEach(l => Console.WriteLine("    " + string.Join(" or ", l.Names)));

			return true;
		}
	}
}
