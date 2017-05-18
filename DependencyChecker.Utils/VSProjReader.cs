using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace org.pescuma.dependencychecker.utils
{
	// http://stackoverflow.com/questions/4649989/reading-a-csproj-file-in-c-sharp
	[DebuggerDisplay("{Name}")]
	public class VSProjReader
	{
		private readonly string folder;
		private readonly XDocument xdoc;

		public readonly string Name;
		public readonly string Filename;

		private static readonly XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

		public VSProjReader(string csproj)
		{
			folder = Path.GetDirectoryName(csproj);
			Name = Path.GetFileNameWithoutExtension(csproj);
			Filename = Path.GetFullPath(csproj);

			try
			{
				xdoc = XDocument.Load(csproj, LoadOptions.SetLineInfo);
			}
			catch (Exception e)
			{
				throw new IOException("Error reading '" + csproj + "': " + e.Message, e);
			}

			if (xdoc.Root == null || xdoc.Root.Name != ns + "Project")
				throw new IOException("Invalid csproj file: " + csproj);
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

		public string OutputType
		{
			get
			{
				return Nodes("OutputType")
						.Select(n => n.Value)
						.First();
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

		public IEnumerable<COMReference> COMReferences
		{
			get
			{
				return Nodes("COMReference")
						.Select(n => new COMReference(n));
			}
		}

		public IEnumerable<string> Contents
		{
			get
			{
				List<string> ignored = Nodes("WCFMetadataStorage")
						.Select(c => Attribute(c, "Include"))
						.Where(i => !string.IsNullOrWhiteSpace(i))
						.Select(i =>
						{
							if (!i.EndsWith("\\"))
								return i + "\\";
							else
								return i;
						})
						.ToList();

				return Nodes("Content")
						.Concat(Nodes("None"))
						.Select(c => Attribute(c, "Include"))
						.Where(c => !string.IsNullOrWhiteSpace(c) && !ignored.Any(i => c.StartsWith(i, StringComparison.CurrentCultureIgnoreCase)))
						.Select(c => Path.Combine(folder, c));
			}
		}

		public IEnumerable<string> OutputFiles
		{
			get
			{
				string extension = ToExtension(OutputType);
				if (extension == null)
					return new List<string>();

				string assemblyName = AssemblyName + extension;

				return Nodes("OutputPath")
						.Select(c =>
						{
							string path = Path.Combine(folder, c.Value, assemblyName);
							return Path.GetFullPath(path);
						});
			}
		}
		public IEnumerable<string> DocumentationFiles
		{
			get
			{
				return Nodes("DocumentationFile")
						.Select(c =>
						{
							string path = Path.Combine(folder, c.Value);
							return Path.GetFullPath(path);
						});
			}
		}

		private string ToExtension(string outputType)
		{
			if ("Exe".Equals(outputType, StringComparison.InvariantCultureIgnoreCase))
				return ".exe";

			else if ("WinExe".Equals(outputType, StringComparison.InvariantCultureIgnoreCase))
				return ".exe";

			else if ("Library".Equals(outputType, StringComparison.InvariantCultureIgnoreCase))
				return ".dll";

			else
				return null;
		}

		private IEnumerable<XElement> Nodes(string name)
		{
			return xdoc.Descendants(ns + name);
		}

		private static XElement Node(XElement node, string name)
		{
			return node.Element(ns + name);
		}

		private static string Attribute(XElement node, string name)
		{
			XAttribute attr = node.Attribute(name);
			if (attr == null)
				return null;

			return attr.Value;
		}

		public class Reference
		{
			private readonly VSProjReader reader;
			private readonly XElement node;

			public Reference(VSProjReader reader, XElement node)
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
					XElement hintPath = Node(node, "HintPath");
					if (hintPath == null)
						return null;

					string result = hintPath.Value;
					if (string.IsNullOrWhiteSpace(result))
						return null;

					return PathUtils.ToAbsolute(reader.folder, result);
				}
			}
		}

		public class COMReference
		{
			private readonly XElement node;

			public COMReference(XElement node)
			{
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

			public Guid Guid
			{
				get
				{
					return new Guid(Node(node, "Guid")
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

					return result;
				}
			}
		}

		public class ProjectReference
		{
			private readonly VSProjReader reader;
			private readonly XElement node;

			public ProjectReference(VSProjReader reader, XElement node)
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

					return PathUtils.ToAbsolute(reader.folder, result);
				}
			}
		}
	}
}
