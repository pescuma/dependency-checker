using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace org.pescuma.dotnetdependencychecker
{
	// http://stackoverflow.com/questions/4649989/reading-a-csproj-file-in-c-sharp
	public class CsprojReader
	{
		private readonly string folder;
		private readonly XDocument xdoc;

		public readonly string Name;
		public readonly string Filename;

		public CsprojReader(string csproj)
		{
			folder = Path.GetDirectoryName(csproj);
			Name = Path.GetFileNameWithoutExtension(csproj);
			Filename = Path.GetFullPath(csproj);

			xdoc = XDocument.Load(csproj, LoadOptions.SetLineInfo);
		}

		public Guid ProjectGuid
		{
			get
			{
				return new Guid(Nodes("ProjectGuid")
					.Select(n => n.Value)
					.First());
			}
		}

		public string AssemblyName
		{
			get
			{
				return Nodes("AssemblyName")
					.Select(n => n.Value)
					.First();
			}
		}

		public IEnumerable<Reference> References
		{
			get
			{
				return Nodes("Reference")
					.Select(n => new Reference(this, n));
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

		private IEnumerable<XElement> Nodes(string name)
		{
			XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
			return xdoc.Descendants(ns + name);
		}

		private static XElement Node(XElement node, string name)
		{
			XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
			return node.Element(ns + name);
		}

		private static string Attribute(XElement node, string name)
		{
			var attr = node.Attribute(name);
			if (attr == null)
				return null;

			return attr.Value;
		}

		public class Reference
		{
			private readonly CsprojReader reader;
			private readonly XElement node;

			public Reference(CsprojReader reader, XElement node)
			{
				this.reader = reader;
				this.node = node;
			}

			public int LineNumber
			{
				get
				{
					var info = (IXmlLineInfo) node;
					return info.LineNumber;
				}
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

			public string HintPath
			{
				get
				{
					var result = Node(node, "HintPath");
					if (result == null)
						return null;

					var path = result.Value;
					if (string.IsNullOrWhiteSpace(path))
						return null;

					if (!Path.IsPathRooted(path))
						path = Path.Combine(reader.folder, path);

					return Path.GetFullPath(path);
				}
			}
		}

		public class ProjectReference
		{
			private readonly CsprojReader reader;
			private readonly XElement node;

			public ProjectReference(CsprojReader reader, XElement node)
			{
				this.reader = reader;
				this.node = node;
			}

			public int LineNumber
			{
				get
				{
					var info = (IXmlLineInfo) node;
					return info.LineNumber;
				}
			}

			public string Name
			{
				get { return Path.GetFileNameWithoutExtension(Include); }
			}

			public Guid ProjectGuid
			{
				get
				{
					return new Guid(Node(node, "Project")
						.Value);
				}
			}

			public string Include
			{
				get
				{
					var result = Attribute(node, "Include");
					if (string.IsNullOrWhiteSpace(result))
						return null;

					if (!Path.IsPathRooted(result))
						result = Path.Combine(reader.folder, result);

					return Path.GetFullPath(result);
				}
			}
		}
	}
}
