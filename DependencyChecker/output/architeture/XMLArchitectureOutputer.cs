﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using org.pescuma.dependencychecker.architecture;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.output.architeture
{
	public class XMLArchitectureOutputer : ArchitectureOutputer
	{
		private readonly string file;

		public XMLArchitectureOutputer(string file)
		{
			this.file = file;
		}

		public void Output(ArchitectureGraph architecture, List<OutputEntry> warnings)
		{
			var xdoc = new XDocument();
			var xroot = new XElement("Architecture");
			xdoc.Add(xroot);

			foreach (var group in architecture.Vertices.OrderBy(v => v, StringComparer.CurrentCultureIgnoreCase))
				xroot.Add(new XElement("Group", new XAttribute("Name", group)));

			foreach (var dep in architecture.Edges.Sort(GroupDependency.NaturalOrdering))
				xroot.Add(new XElement("Dependency", //
					new XAttribute("Source", dep.Source), //
					new XAttribute("Target", dep.Target), //
					new XAttribute("Implicit", dep.Implicit), //
					new XAttribute("Conflicted", dep.Conflicted)));

			File.WriteAllText(file, xdoc.ToString());
		}
	}
}