using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using org.pescuma.dependencychecker.model.xml;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.output.results
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
					xentry.Add(xprojs);
					XMLHelper.ToXML(entry.Projects)
						.ForEach(xprojs.Add);
				}

				if (entry.Dependencies.Any())
				{
					var xdeps = new XElement("Dependencies");
					xentry.Add(xdeps);
					XMLHelper.ToXML(entry.Dependencies)
						.ForEach(xdeps.Add);
				}
			}

			File.WriteAllText(file, xdoc.ToString());
		}
	}
}
