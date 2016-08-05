using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.config;
using org.pescuma.dependencychecker.presenter.rules;
using org.pescuma.dependencychecker.presenter.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal abstract class BaseCommand : Command
	{
		public abstract string Name { get; }

		public bool Handle(string line, DependencyGraph graph)
		{
			if (!IsCommandOrShortcut(line, Name))
				return false;

			var args = RemoveCommand(line, Name);

			var result = new Output("    ");
			InternalHandle(result, args, graph);

			result.ToConsole();

			return true;
		}

		protected abstract void InternalHandle(Output result, string args, DependencyGraph graph);

		private bool IsCommandOrShortcut(string line, string name)
		{
			if (line == name)
				return true;
			if (line == name.Substring(0, 1))
				return true;
			if (line.StartsWith(name + " "))
				return true;
			if (line.StartsWith(name.Substring(0, 1) + " "))
				return true;
			return false;
		}

		private string RemoveCommand(string line, string name)
		{
			if (line.StartsWith(name + " "))
				return line.Substring(name.Length + 1)
					.Trim();

			if (line.StartsWith(name.Substring(0, 1) + " "))
				return line.Substring(2)
					.Trim();

			return "";
		}

		protected List<Library> FilterLibs(DependencyGraph graph, string search)
		{
			var libs = graph.Vertices;

			if (search != "")
			{
				LibraryMatcher matcher = ParseLibraryOrGroupMatcher(search);

				libs = libs.Where(l => matcher(l, Matchers.NullReporter));
			}

			return libs.ToList();
		}

		protected string GetName(Library l)
		{
			return string.Join(" or ", l.SortedNames);
		}

		protected LibraryMatcher ParseLibraryOrGroupMatcher(string search)
		{
			LibraryMatcher proj = new ConfigParser().ParseLibraryMatcher(search, new ConfigLocation(1, search));

			List<Func<string, bool>> ms = Matchers.CreateStringMatchers(search);
			LibraryMatcher group = (l, r) => l.GroupElement != null && ms.Any(m => m(l.GroupElement.Name));

			return (l, r) => proj(l, r) || group(l, r);
		}
	}
}
