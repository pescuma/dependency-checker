using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencyconsole.commands
{
	internal interface Command
	{
		string Name { get; }

		bool Handle(string line, DependencyGraph graph);
	}
}
