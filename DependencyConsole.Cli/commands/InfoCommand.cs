using System;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class InfoCommand : BaseCommand
	{
		public override string Name
		{
			get { return "info"; }
		}

		protected override void InternalHandle(string args, DependencyGraph graph)
		{
			var libs = FilterLibs(graph, args);

			if (!libs.Any())
				Console.WriteLine("No libraries found");
			else
				libs.SortBy(Library.NaturalOrdering)
					.ForEach(OutputInfo);
		}

		private void OutputInfo(Library lib)
		{
			Console.WriteLine(GetName(lib) + ":");

			var proj = lib as Project;
			if (proj != null)
			{
				WriteProperty("Type", "Project");
				WriteProperty("Project name", proj.ProjectName);
				WriteProperty("Library name", proj.LibraryName);
				WriteProperty("Project path", proj.ProjectPath);
				WriteProperty("GUID", proj.Guid != null ? proj.Guid.ToString() : "missing");
			}
			else
			{
				WriteProperty("Type", "Library");
				WriteProperty("Library name", lib.LibraryName);
			}

			if (lib.GroupElement != null)
				WriteProperty("Group", lib.GroupElement.Name);

			lib.Languages.ForEach(p => WriteProperty("Language", p));

			var projectPath = (lib is Project ? ((Project) lib).ProjectPath : null);
			lib.Paths.Where(p => p != projectPath)
				.ForEach(p => WriteProperty("Path", p));
		}

		private void WriteProperty(string name, string value)
		{
			Console.WriteLine(PREFIX + name + ": " + value);
		}
	}
}
