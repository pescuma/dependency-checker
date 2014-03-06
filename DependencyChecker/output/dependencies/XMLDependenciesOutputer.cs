using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencychecker.output.dependencies
{
	public class XMLDependenciesOutputer : DependenciesOutputer
	{
		private readonly string file;

		public XMLDependenciesOutputer(string file)
		{
			this.file = file;
		}

		public void Output(DependencyGraph graph, List<OutputEntry> warnings)
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
			var projs = graph.Vertices.ToList();
			projs.Sort(Library.NaturalOrdering);

			foreach (var library in projs)
			{
				var el = new XElement("Element");
				xroot.Add(el);

				if (library is Project)
				{
					var proj = (Project) library;
					el.Add(new XAttribute("Type", "Project"));
					el.Add(new XAttribute("Name", proj.ProjectName));
					el.Add(new XAttribute("LibraryName", proj.LibraryName));
					el.Add(new XAttribute("ProjectPath", proj.ProjectPath));
					if (proj.Guid != null)
						el.Add(new XAttribute("GUID", proj.Guid.Value));
				}
				else
				{
					el.Add(new XAttribute("Type", "Library"));
					el.Add(new XAttribute("Name", library.LibraryName));
					library.Paths.ForEach(p => el.Add(new XAttribute("Path", p)));
				}

				if (library.GroupElement != null)
					el.Add(new XAttribute("Group", library.GroupElement.Name));
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

				el.Add(new XAttribute("Source", dep.Source.Name));
				el.Add(new XAttribute("Target", dep.Target.Name));
				el.Add(new XAttribute("ReferenceType", dep.Type == Dependency.Types.LibraryReference ? "Library" : "Project"));

				if (dep.ReferencedPath != null)
					el.Add(new XAttribute("ReferencedPath", dep.ReferencedPath));
			}
		}
	}
}
