using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.model.xml;

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

			XMLHelper.ToXML(graph.Vertices)
				.ForEach(xroot.Add);

			XMLHelper.ToXML(graph.Edges)
				.ForEach(xroot.Add);

			File.WriteAllText(file, xdoc.ToString());
		}
	}
}
