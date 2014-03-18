using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.config;
using org.pescuma.dependencychecker.presenter.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal abstract class BaseConfigCommand : Command
	{
		public abstract string Name { get; }

		public bool Handle(string line, DependencyGraph graph)
		{
			if (!line.StartsWith(Name))
				return false;

			var result = new Output("    ");
			InternalHandle(result, new ConfigParser().ParseLines("-", new[] { line }), graph);

			result.ToConsole();

			return true;
		}

		protected abstract void InternalHandle(Output result, Config config, DependencyGraph graph);
	}
}
