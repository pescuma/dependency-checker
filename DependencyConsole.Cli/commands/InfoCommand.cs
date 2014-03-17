using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;
using org.pescuma.dependencyconsole.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class InfoCommand : BaseCommand
	{
		public override string Name
		{
			get { return "info"; }
		}

		protected override void InternalHandle(Output result, string args, DependencyGraph graph)
		{
			var libs = FilterLibs(graph, args);

			if (!libs.Any())
				result.AppendLine("No libraries found");
			else
				libs.SortBy(Library.NaturalOrdering)
					.ForEach(l => OutputInfo(result, l));
		}

		private void OutputInfo(Output result, Library lib)
		{
			result.AppendLine(GetName(lib) + ":");
			result.IncreaseIndent();

			var proj = lib as Project;
			if (proj != null)
			{
				WriteProperty(result, "Type", "Project");
				WriteProperty(result, "Project name", proj.ProjectName);
				WriteProperty(result, "Library name", proj.LibraryName);
				WriteProperty(result, "Project path", proj.ProjectPath);
				WriteProperty(result, "GUID", proj.Guid != null ? proj.Guid.ToString() : "missing");
			}
			else
			{
				WriteProperty(result, "Type", "Library");
				WriteProperty(result, "Library name", lib.LibraryName);
			}

			if (lib.GroupElement != null)
				WriteProperty(result, "Group", lib.GroupElement.Name);

			lib.Languages.ForEach(p => WriteProperty(result, "Language", p));

			var projectPath = (lib is Project ? ((Project) lib).ProjectPath : null);
			lib.Paths.Where(p => p != projectPath)
				.ForEach(p => WriteProperty(result, "Path", p));

			result.DecreaseIndent();
			result.AppendLine();
		}

		private void WriteProperty(Output result, string name, string value)
		{
			result.AppendLine(name + ": " + value);
		}
	}
}
