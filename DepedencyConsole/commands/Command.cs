namespace org.pescuma.dependencychecker.commands
{
	internal interface Command
	{
		string Name { get; }

		bool Handle(string line, DependencyGraph graph);
	}
}
