using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.architecture;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.output.dependencies
{
	public class TextDependenciesOutputer : DependenciesOutputer
	{
		private readonly string file;

		public TextDependenciesOutputer(string file)
		{
			this.file = file;
		}

		public void Output(DependencyGraph graph, ArchitectureGraph architecture, List<OutputEntry> warnings)
		{
			var result = new StringBuilder();
			AppendProjects(result, graph);
			AppendDependencies(result, graph);
			File.WriteAllText(file, result.ToString());
		}

		private void AppendProjects(StringBuilder result, DependencyGraph graph)
		{
			List<Library> projs = graph.Vertices.ToList();
			projs.Sort(Library.NaturalOrdering);

			foreach (Library lib in projs)
			{
				var proj = lib as Project;

				if (proj != null)
				{
					result.Append("Project ")
							.Append(proj.ProjectName)
							.Append("\n");
					AppendProperty(result, "Library name", proj.LibraryName);
					if (proj.ProjectPath != null)
						AppendProperty(result, "Project path", proj.ProjectPath);
					if (proj.Guid != null)
						AppendProperty(result, "GUID", proj.Guid.Value);
				}
				else
				{
					result.Append("Library ")
							.Append(lib.LibraryName)
							.Append("\n");
				}

				if (lib.GroupElement != null)
					AppendProperty(result, "Group", lib.GroupElement.Name);

				lib.Languages.ForEach(p => AppendProperty(result, "Language", p));

				if (proj != null)
				{
					proj.OutputPaths.ForEach(p => AppendProperty(result, "Output path", p));
					proj.DocumentationPaths.ForEach(p => AppendProperty(result, "Documentation path", p));
				}

				string projectPath = (proj != null ? proj.ProjectPath : null);
				lib.Paths.Where(p => p != projectPath)
						.ForEach(p => AppendProperty(result, "Path", p));

				result.Append("\n");
			}
		}

		private void AppendDependencies(StringBuilder result, DependencyGraph graph)
		{
			List<Dependency> deps = graph.Edges.ToList();
			deps.Sort(Dependency.NaturalOrdering);

			foreach (Dependency dep in deps)
			{
				result.Append("Dependency: ")
						.Append(dep.Source.Name)
						.Append(" depends on ")
						.Append(dep.Target.Name)
						.Append("\n");

				AppendProperty(result, "Reference type", dep.Type == Dependency.Types.LibraryReference ? "library" : "project");

				if (dep.ReferencedPath != null)
					AppendProperty(result, "Referenced path", dep.ReferencedPath);

				result.Append("\n");
			}
		}

		private void AppendProperty(StringBuilder result, string name, object value)
		{
			result.Append("  - ")
					.Append(name)
					.Append(": ")
					.Append(value)
					.Append("\n");
		}
	}
}
