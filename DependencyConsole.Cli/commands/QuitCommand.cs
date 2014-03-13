using System;
using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencyconsole.commands
{
	internal class QuitCommand : BaseCommand
	{
		public override string Name
		{
			get { return "quit"; }
		}

		protected override void InternalHandle(string args, DependencyGraph graph)
		{
			throw new QuitException();
		}
	}

	internal class QuitException : Exception
	{
	}
}
