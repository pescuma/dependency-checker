using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.output.dependencies
{
	public class TextDependenciesOutputer : DependenciesOutputer
	{
		private readonly string file;

		public TextDependenciesOutputer(string file)
		{
			this.file = file;
		}

		public void Output(DependencyGraph graph)
		{
			var result = new StringBuilder();
			AppendProjects(result, graph);
			AppendDependencies(result, graph);
			File.WriteAllText(file, result.ToString());
		}

		private void AppendProjects(StringBuilder result, DependencyGraph graph)
		{
			var projs = graph.Vertices.OfType<Assembly>()
				.ToList();
			projs.Sort(DependableUtils.NaturalOrdering);

			foreach (var assembly in projs)
			{
				if (assembly is Project)
				{
					var proj = (Project) assembly;
					result.Append("Project ")
						.Append(proj.Name)
						.Append("\n");
					AppendProperty(result, "Assembly name", proj.AssemblyName);
					AppendProperty(result, "csproj path", proj.CsprojPath);
					AppendProperty(result, "GUID", proj.Guid);
				}
				else
				{
					result.Append("Assembly ")
						.Append(assembly.AssemblyName)
						.Append("\n");
					assembly.Paths.ForEach(p => AppendProperty(result, "Path", p));
				}

				if (assembly.GroupElement != null)
					AppendProperty(result, "Group", assembly.GroupElement.Name);

				result.Append("\n");
			}
		}

		private void AppendDependencies(StringBuilder result, DependencyGraph graph)
		{
			var deps = graph.Edges.ToList();
			deps.Sort(Dependency.NaturalOrdering);

			foreach (var dep in deps)
			{
				result.Append("Dependency: ")
					.Append(dep.Source.Names.First())
					.Append(" depends on ")
					.Append(dep.Target.Names.First())
					.Append("\n");

				AppendProperty(result, "Reference type", dep.Type == Dependency.Types.DllReference ? "DLL" : "project");

				if (dep.DLLHintPath != null)
					AppendProperty(result, "DLL path", dep.DLLHintPath);

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
