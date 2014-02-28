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

		private XElement CreateXProject(Dependable dependable)
		{
			var result = new XElement("Element");

			if (dependable is GroupElement)
				dependable = ((GroupElement) dependable).Representing;

			if (dependable is Project)
			{
				var proj = (Project) dependable;
				result.Add(new XAttribute("Type", "Project"));
				result.Add(new XAttribute("Name", proj.Name));
				result.Add(new XAttribute("AssemblyName", proj.AssemblyName));
				result.Add(new XAttribute("Guid", proj.Guid.ToString()));
				result.Add(new XAttribute("Csproj", proj.CsprojPath));
			}
			else if (dependable is Assembly)
			{
				var assembly = (Assembly) dependable;
				result.Add(new XAttribute("Type", "Assembly"));
				result.Add(new XAttribute("AssemblyName", assembly.AssemblyName));
			}
			else
			{
				throw new InvalidDataException();
			}

			var group = ((Assembly) dependable).GroupElement;
			if (group != null)
			{
				var xgroup = new XElement("Group");
				result.Add(xgroup);

				xgroup.Add(new XAttribute("Name", group.Name));
				xgroup.Add(new XElement("Rule", //
					new XAttribute("Line", group.Location.LineNum), //
					group.Location.LineText));
			}

			dependable.Paths.ForEach(p => result.Add(new XElement("Path", p)));

			return result;
		}

		private XElement CreateXDependency(Dependency dep)
		{
			var result = new XElement("Dependency");

			result.Add(new XAttribute("Type", dep.Type.ToString()));
			result.Add(new XAttribute("Source", GetProjectName(dep.Source)));
			result.Add(new XAttribute("Target", GetProjectName(dep.Target)));
			result.Add(new XElement("Location", //
				new XAttribute("File", dep.Location.File), //
				new XAttribute("Line", dep.Location.Line)));
			if (dep.DLLHintPath != null)
				result.Add(new XElement("DLLHintPath", dep.DLLHintPath));

			return result;
		}

		private string GetProjectName(Dependable source)
		{
			if (source is GroupElement)
				return GetProjectName(((GroupElement) source).Representing);

			return source.Names.First();
		}
	}
}
