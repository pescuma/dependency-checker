using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.model.xml
{
	public class XMLHelper
	{
		public static List<XElement> ToXML(IEnumerable<Library> libraries)
		{
			return libraries.SortBy(Library.NaturalOrdering)
				.Select(ToXML)
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

			library.SortedNames.ForEach(p => result.Add(new XElement("Name", p)));
			library.Languages.ForEach(p => result.Add(new XElement("Language", p)));
			library.Paths.ForEach(p => result.Add(new XElement("Path", p)));

			return result;
		}

		private static string Attr(XElement el, string attribute)
		{
			var result = el.Attribute(attribute);
			if (result == null)
				return null;
			else
				return result.Value;
		}

		private static string Element(XElement el, string attribute)
		{
			var result = el.Element(attribute);
			if (result == null)
				return null;
			else
				return result.Value;
		}

		public static Library LibraryFromXML(XElement el)
		{
			Argument.ThrowIfFalse("Element" == el.Name);

			var groups = new Dictionary<string, Group>();

			Library result;

			var type = Attr(el, "Type");
			if (type == "Project")
			{
				var projectName = Attr(el, "ProjectName");
				var libraryName = Attr(el, "LibraryName");
				var projectPath = Attr(el, "ProjectPath");
				var guid = Attr(el, "Guid");

				result = new Project(projectName, libraryName, guid != null ? new Guid(guid) : (Guid?) null, projectPath, null);
			}
			else if (type == "Library")
			{
				var libraryName = Attr(el, "LibraryName");

				result = new Library(libraryName, null);
			}
			else
				throw new IOException("Invalid type: " + type);

			var xgroup = el.Element("Group");
			if (xgroup != null)
			{
				var name = Attr(xgroup, "Name");

				var rule = xgroup.Element("Rule");
				if (rule == null)
					throw new IOException("Missing Rule inside Group");

				var configLine = int.Parse(Attr(rule, "Line"));
				var configText = rule.Value;

				var group = GetGroup(groups, name);

				result.GroupElement = new GroupElement(group, new ConfigLocation(configLine, configText), result);
			}

			result.Names.AddRange(el.Descendants("Name")
				.Select(e => e.Value));
			result.Languages.AddRange(el.Descendants("Language")
				.Select(e => e.Value));
			result.Paths.AddRange(el.Descendants("Path")
				.Select(e => e.Value));

			return result;
		}

		private static Group GetGroup(Dictionary<string, Group> groups, string name)
		{
			var result = groups.Get(name);
			if (result == null)
			{
				result = new Group(name);
				groups.Add(name, result);
			}
			return result;
		}

		public static List<XElement> ToXML(IEnumerable<Dependency> deps)
		{
			return deps.SortBy(Dependency.NaturalOrdering)
				.Select(ToXML)
				.ToList();
		}

		public static XElement ToXML(Dependency dep)
		{
			var result = new XElement("Dependency");

			result.Add(new XAttribute("Type", dep.Type.ToString()));

			result.Add(ToXMLAsKey("Source", dep.Source));
			result.Add(ToXMLAsKey("Target", dep.Target));

			result.Add(new XElement("Location", //
				new XAttribute("File", dep.Location.File), //
				new XAttribute("Line", dep.Location.Line)));

			if (dep.ReferencedPath != null)
				result.Add(new XElement("ReferencedPath", dep.ReferencedPath));

			return result;
		}

		public static Dependency DependencyFromXML(XElement el, Dictionary<string, Library> libsByKey)
		{
			var typeName = Attr(el, "Type");
			Dependency.Types type;
			if (!Enum.TryParse(typeName, out type))
				throw new IOException("Wrong type: " + typeName);

			var source = FromXMLAsKey(el.Element("Source"), libsByKey);
			var target = FromXMLAsKey(el.Element("Target"), libsByKey);

			var xlocation = el.Element("Location");
			var locFile = Attr(xlocation, "File");
			var locLine = int.Parse(Attr(xlocation, "Line"));

			var referencedPath = Element(el, "ReferencedPath");

			return new Dependency(source, target, type, new Location(locFile, locLine), referencedPath);
		}

		public static void ToXML(XElement xroot, DependencyGraph graph)
		{
			ToXML(graph.Vertices)
				.ForEach(xroot.Add);

			ToXML(graph.Edges)
				.ForEach(xroot.Add);
		}

		public static DependencyGraph DependencyGraphFromXML(XElement xroot)
		{
			var libs = xroot.Descendants("Element")
				.Select(LibraryFromXML)
				.ToList();

			var libsByKey = libs.ToDictionary(ToKey, l => l);

			var deps = xroot.Descendants("Dependency")
				.Select(el => DependencyFromXML(el, libsByKey));

			var graph = new DependencyGraph();
			graph.AddVertexRange(libs);
			graph.AddEdgeRange(deps);
			return graph;
		}

		private static XElement ToXMLAsKey(string source, Library lib)
		{
			var el = new XElement(source);

			if (lib is Project)
			{
				var proj = (Project) lib;
				el.Add(new XAttribute("ProjectName", proj.ProjectName));
				el.Add(new XAttribute("LibraryName", proj.LibraryName));
				if (proj.ProjectPath != null)
					el.Add(new XAttribute("ProjectPath", proj.ProjectPath));
			}
			else
			{
				el.Add(new XAttribute("LibraryName", lib.LibraryName));
			}

			return el;
		}

		private static Library FromXMLAsKey(XElement el, Dictionary<string, Library> libsByKey)
		{
			var projectName = Attr(el, "ProjectName");
			var libraryName = Attr(el, "LibraryName");
			var projectPath = Attr(el, "ProjectPath");

			var key = ToKey(projectName, libraryName, projectPath);

			return libsByKey[key];
		}

		private static string ToKey(Library lib)
		{
			var proj = lib as Project;

			if (proj != null)
				return ToKey(proj.ProjectName, proj.LibraryName, proj.ProjectPath);
			else
				return ToKey(null, lib.LibraryName, null);
		}

		private static string ToKey(string projectName, string libraryName, string projectPath)
		{
			if (projectName == null)
				return libraryName;
			else
				return projectName + "\n" + libraryName + "\n" + (projectPath ?? "<null>");
		}
	}
}
