using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using org.pescuma.dependencychecker.output;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.input.loaders
{
	public class EclipseProjectsLoader : ProjectLoader
	{
		public void LoadProjects(List<string> paths, DependencyGraphBuilder builder, List<OutputEntry> warnings)
		{
			var projectFiles = new HashSet<string>(paths.SelectMany(folder => Directory.GetFiles(folder, ".project", SearchOption.AllDirectories))
				.Select(Path.GetFullPath));
			foreach (var projectFile in projectFiles)
			{
// ReSharper disable once AssignNullToNotNullAttribute
				var classpathFile = Path.Combine(Path.GetDirectoryName(projectFile), ".classpath");
				LoadProject(builder, projectFile, classpathFile);
			}
		}

		private void LoadProject(DependencyGraphBuilder builder, string projectFile, string classpathFile)
		{
			var xproject = XDocument.Load(projectFile, LoadOptions.SetLineInfo);
			if (xproject.Root == null || xproject.Root.Name != "projectDescription")
				throw new IOException("Invalid .project file: " + projectFile);

			XDocument xclasspath = null;
			if (File.Exists(classpathFile))
			{
				xclasspath = XDocument.Load(classpathFile, LoadOptions.SetLineInfo);
				if (xclasspath.Root == null || xclasspath.Root.Name != "classpath")
					throw new IOException("Invalid .classpath file: " + classpathFile);
			}

			var name = xproject.XPathSelectElement("/projectDescription/name")
				.Value;

			var basePath = Path.GetDirectoryName(classpathFile);

			var proj = builder.AddProject(name, name, null, projectFile);

			var referencedProjects = AddReferencesFromClassPath(builder, proj, xclasspath, basePath, classpathFile);

			foreach (var xtarget in xproject.XPathSelectElements("/projectDescription/projects/project"))
			{
				var targetProjectName = xtarget.Value;
				if (!referencedProjects.Contains(targetProjectName))
					builder.AddProjectReference(proj, targetProjectName, null, null, null, ToLocation(classpathFile, xtarget));
			}
		}

		private static HashSet<string> AddReferencesFromClassPath(DependencyGraphBuilder builder, object proj, XDocument xclasspath,
			string basePath, string classpathFile)
		{
			var referencedProjects = new HashSet<string>();

			if (xclasspath != null)
			{
				foreach (var xcpe in xclasspath.Descendants("classpathentry"))
				{
					var kind = xcpe.Attribute("kind")
						.Value;
					var path = xcpe.Attribute("path")
						.Value;

					if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(path))
						continue;

					if (kind == "lib")
					{
						var targetPath = PathUtils.ToAbsolute(basePath, path);
						var targetName = GuessLibraryName(targetPath);
						builder.AddLibraryReference(proj, null, targetName, null, targetPath, ToLocation(classpathFile, xcpe));
					}
					else if (kind == "src" && path.StartsWith("/"))
					{
						var targetProjectName = path.Substring(1);
						builder.AddProjectReference(proj, targetProjectName, null, null, null, ToLocation(classpathFile, xcpe));
						referencedProjects.Add(targetProjectName);
					}
				}
			}
			return referencedProjects;
		}

		public static string GuessLibraryName(string name)
		{
			name = Path.GetFileNameWithoutExtension(name);
			name = RemoveOneSuffix(name);
			name = RemoveVersion(name);
			return name;
		}

		private static string[] suffixes = { "-jar", "-bundle", "-source" };

		private static string RemoveOneSuffix(string name)
		{
			foreach (var suffix in suffixes)
				if (name.EndsWith(suffix))
					return name.Substring(0, name.Length - suffix.Length);
			return name;
		}

		private static Regex versionRegex = new Regex(@"-\d{1,2}(\.\d{1,3}){1,3}$", RegexOptions.IgnoreCase);

		private static string RemoveVersion(string name)
		{
			var ms = versionRegex.Matches(name);
			if (ms.Count > 0)
				return name.Substring(0, ms[0].Index);
			else
				return name;
		}

		private static Location ToLocation(string classpathFile, XElement xcpe)
		{
			return new Location(classpathFile, ((IXmlLineInfo) xcpe).LineNumber);
		}
	}
}
