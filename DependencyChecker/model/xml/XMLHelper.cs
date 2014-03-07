using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.model.xml
{
	public class XMLHelper
	{
		public static List<XElement> ToXML(IEnumerable<Library> libraries)
		{
			var libs = libraries.ToList();
			libs.Sort(Library.NaturalOrdering);
			return libs.Select(ToXML)
				.ToList();
		}

		public static XElement ToXML(Library library)
		{
			var result = new XElement("Element");

			if (library is Project)
			{
				var proj = (Project) library;
				result.Add(new XAttribute("Type", "Project"));
				result.Add(new XAttribute("ProjectName", proj.Name));
				result.Add(new XAttribute("LibraryName", proj.LibraryName));
				if (proj.ProjectPath != null)
					result.Add(new XAttribute("ProjectPath", proj.ProjectPath));
				if (proj.Guid != null)
					result.Add(new XAttribute("Guid", proj.Guid.Value));
			}
			else
			{
				result.Add(new XAttribute("Type", "Library"));
				result.Add(new XAttribute("LibraryName", library.LibraryName));
			}

			if (library.GroupElement != null)
			{
				var xgroup = new XElement("Group");
				result.Add(xgroup);

				xgroup.Add(new XAttribute("Name", library.GroupElement.Name));
				xgroup.Add(new XElement("Rule", //
					new XAttribute("Line", library.GroupElement.Location.LineNum), //
					library.GroupElement.Location.LineText));
			}

			library.Languages.ForEach(p => result.Add(new XElement("Language", p)));
			library.Paths.ForEach(p => result.Add(new XElement("Path", p)));

			return result;
		}

		public static List<XElement> ToXML(IEnumerable<Dependency> deps)
		{
			var ds = deps.ToList();
			ds.Sort(Dependency.NaturalOrdering);
			return ds.Select(ToXML)
				.ToList();
		}

		public static XElement ToXML(Dependency dep)
		{
			var result = new XElement("Dependency");

			result.Add(new XAttribute("Source", dep.Source.Name));
			result.Add(new XAttribute("Target", dep.Target.Name));
			result.Add(new XAttribute("Type", dep.Type.ToString()));

			result.Add(new XElement("Location", //
				new XAttribute("File", dep.Location.File), //
				new XAttribute("Line", dep.Location.Line)));

			if (dep.ReferencedPath != null)
				result.Add(new XElement("ReferencedPath", dep.ReferencedPath));

			return result;
		}
	}
}
