using System.IO;
using System.Linq;
using System.Xml.Linq;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.output.dependencies
{
	public class XMLDependenciesOutputer : DependenciesOutputer
	{
		private readonly string file;

		public XMLDependenciesOutputer(string file)
		{
			this.file = file;
		}

		public void Output(DependencyGraph graph)
		{
			var xdoc = new XDocument();
			var xroot = new XElement("Depedencies");
			xdoc.Add(xroot);

			AppendProjects(xroot, graph);
			AppendDependencies(xroot, graph);

			File.WriteAllText(file, xdoc.ToString());
		}

		private void AppendProjects(XElement xroot, DependencyGraph graph)
		{
			var projs = graph.Vertices.OfType<Assembly>()
				.ToList();
			projs.Sort(DependableUtils.NaturalOrdering);

			foreach (var assembly in projs)
			{
				var el = new XElement("Element");
				xroot.Add(el);

				if (assembly is Project)
				{
					var proj = (Project) assembly;
					el.Add(new XAttribute("Type", "Project"));
					el.Add(new XAttribute("Name", proj.Name));
					el.Add(new XAttribute("AssemblyName", proj.AssemblyName));
					el.Add(new XAttribute("CsprojPath", proj.CsprojPath));
					el.Add(new XAttribute("GUID", proj.Guid));
				}
				else
				{
					el.Add(new XAttribute("Type", "Assembly"));
					el.Add(new XAttribute("Name", assembly.AssemblyName));
					assembly.Paths.ForEach(p => el.Add(new XAttribute("Path", p)));
				}

				if (assembly.GroupElement != null)
					el.Add(new XAttribute("Group", assembly.GroupElement.Name));
			}
		}

		private void AppendDependencies(XElement xroot, DependencyGraph graph)
		{
			var deps = graph.Edges.ToList();
			deps.Sort(Dependency.NaturalOrdering);

			foreach (var dep in deps)
			{
				var el = new XElement("Dependency");
				xroot.Add(el);

				el.Add(new XAttribute("Source", dep.Source.Names.First()));
				el.Add(new XAttribute("Target", dep.Target.Names.First()));
				el.Add(new XAttribute("ReferenceType", dep.Type == Dependency.Types.DllReference ? "DLL" : "Project"));

				if (dep.DLLHintPath != null)
					el.Add(new XAttribute("DLLPath", dep.DLLHintPath));
			}
		}
	}
}
