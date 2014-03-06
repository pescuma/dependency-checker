using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.output.results
{
	public class XMLEntryOutputer : EntryOutputer
	{
		private readonly string file;

		public XMLEntryOutputer(string file)
		{
			this.file = file;
		}

		public void Output(List<OutputEntry> entries)
		{
			var xdoc = new XDocument();
			var xroot = new XElement("dotnet-dependency-checker");
			xdoc.Add(xroot);

			var xsummary = new XElement("Summary");
			xroot.Add(xsummary);
			entries.GroupBy(w => w.Severity)
				.ForEach(e => xsummary.Add(new XElement("Severity", //
					new XAttribute("Name", e.Key.ToString()), //
					new XAttribute("Count", e.Count()))));
			entries.GroupBy(w => w.Type)
				.ForEach(e => xsummary.Add(new XElement("Type", //
					new XAttribute("Name", e.Key), //
					new XAttribute("Count", e.Count()))));

			foreach (var entry in entries)
			{
				var xentry = new XElement("Entry");
				xroot.Add(xentry);

				xentry.Add(new XAttribute("Type", entry.Type));
				xentry.Add(new XAttribute("Severity", entry.Severity.ToString()));
				xentry.Add(new XElement("Message", ConsoleEntryOutputer.ToConsole(entry.Messsage)));

				if (entry is DependencyRuleMatch)
				{
					var drm = (DependencyRuleMatch) entry;
					xentry.Add(new XElement("Rule", //
						new XAttribute("Line", drm.Rule.Location.LineNum), //
						drm.Rule.Location.LineText));
				}

				if (entry.Projects.Any())
				{
					var xprojs = new XElement("Projects");
					entry.Projects.ForEach(p => xprojs.Add(CreateXProject(p)));
					xentry.Add(xprojs);
				}

				if (entry.Dependencies.Any())
				{
					var xprojs = new XElement("Dependencies");
					entry.Dependencies.ForEach(d => xprojs.Add(CreateXDependency(d)));
					xentry.Add(xprojs);
				}
			}

			File.WriteAllText(file, xdoc.ToString());
		}

		private XElement CreateXProject(Library library)
		{
			var result = new XElement("Element");

			if (library is Project)
			{
				var proj = (Project) library;
				result.Add(new XAttribute("Type", "Project"));
				result.Add(new XAttribute("Name", proj.Name));
				result.Add(new XAttribute("LibraryName", proj.LibraryName));
				result.Add(new XAttribute("Guid", proj.Guid.ToString()));
				result.Add(new XAttribute("ProjectPath", proj.ProjectPath));
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

			library.Paths.ForEach(p => result.Add(new XElement("Path", p)));

			return result;
		}

		private XElement CreateXDependency(Dependency dep)
		{
			var result = new XElement("Dependency");

			result.Add(new XAttribute("Type", dep.Type.ToString()));
			result.Add(new XAttribute("Source", dep.Source.Name));
			result.Add(new XAttribute("Target", dep.Target.Name));
			result.Add(new XElement("Location", //
				new XAttribute("File", dep.Location.File), //
				new XAttribute("Line", dep.Location.Line)));
			if (dep.ReferencedPath != null)
				result.Add(new XElement("ReferencedPath", dep.ReferencedPath));

			return result;
		}
	}
}
