using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace org.pescuma.dotnetdependencychecker
{
	// http://stackoverflow.com/questions/4649989/reading-a-csproj-file-in-c-sharp
	public class CsprojReader
	{
		private readonly string path;
		private readonly XmlDocument xmldoc;
		private readonly XmlNamespaceManager mgr;

		public readonly string Name;

		public CsprojReader(string csproj)
		{
			path = Path.GetDirectoryName(csproj);
			Name = Path.GetFileNameWithoutExtension(csproj);

			xmldoc = new XmlDocument();
			xmldoc.Load(csproj);

			mgr = new XmlNamespaceManager(xmldoc.NameTable);
			mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");
		}

		public Guid ProjectGuid
		{
			get
			{
				return new Guid(Nodes("ProjectGuid")
					.Select(n => n.InnerText)
					.First());
			}
		}

		public IEnumerable<Reference> References
		{
			get
			{
				return Nodes("Reference")
					.Select(n => new Reference(n));
			}
		}

		public IEnumerable<ProjectReference> ProjectReferences
		{
			get
			{
				return Nodes("ProjectReference")
					.Select(n => new ProjectReference(this, n));
			}
		}

		private IEnumerable<XmlNode> Nodes(string name)
		{
			var result = xmldoc.SelectNodes("//x:" + name, mgr);
			if (result == null)
				return new List<XmlNode>();

			return result.Cast<XmlNode>();
		}

		private static string Attribute(XmlNode node, string name)
		{
			var attributes = node.Attributes;
			if (attributes == null)
				return null;

			var item = attributes.GetNamedItem(name);
			if (item == null)
				return null;

			return item.Value;
		}

		public class Reference
		{
			private readonly XmlNode node;

			public Reference(XmlNode node)
			{
				this.node = node;
			}

			public AssemblyName Include
			{
				get
				{
					var result = Attribute(node, "Include");
					if (result == null)
						return null;

					return new AssemblyName(result);
				}
			}
		}

		public class ProjectReference
		{
			private readonly CsprojReader reader;
			private readonly XmlNode node;

			public ProjectReference(CsprojReader reader, XmlNode node)
			{
				this.reader = reader;
				this.node = node;
			}

			public string Name
			{
				get { return Path.GetFileNameWithoutExtension(Include); }
			}

			public string Include
			{
				get
				{
					var result = Attribute(node, "Include");
					if (result == null)
						return null;

					if (!Path.IsPathRooted(result))
						result = Path.Combine(reader.path, result);

					return Path.GetFullPath(result);
				}
			}
		}
	}
}
