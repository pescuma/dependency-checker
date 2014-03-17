using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencyconsole.commands
{
	internal abstract class BaseCommand : Command
	{
		protected const string PREFIX = "    ";

		public abstract string Name { get; }

		public bool Handle(string line, DependencyGraph graph)
		{
			if (!IsCommandOrShortcut(line, Name))
				return false;

			var args = RemoveCommand(line, Name);
			InternalHandle(args, graph);
			return true;
		}

		protected abstract void InternalHandle(string args, DependencyGraph graph);

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
				var pattern = new Regex("^" + search.Replace(".", "\\.")
					.Replace("*", ".*") + "$", RegexOptions.IgnoreCase);

				libs = libs.Where(l => l.Names.Any(pattern.IsMatch));
			}

			return libs.ToList();
		}

		protected string GetName(Library l)
		{
			return string.Join(" or ", l.SortedNames);
		}
	}
}
