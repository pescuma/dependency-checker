using System;
using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencyconsole.commands
{
	internal class QuitCommand : Command
	{
		public string Name
		{
			get { return "quit"; }
		}

		public bool Handle(string line, DependencyGraph graph)
		{
			if (Name.Equals(line))
				throw new QuitException();

			return false;
		}
	}

	internal class QuitException : Exception
	{
	}
}
