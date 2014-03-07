﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.output.dependencies
{
	public class TextDependenciesOutputer : DependenciesOutputer
	{
		private readonly string file;

		public TextDependenciesOutputer(string file)
		{
			this.file = file;
		}

		public void Output(DependencyGraph graph, List<OutputEntry> warnings)
		{
			var result = new StringBuilder();
			AppendProjects(result, graph);
			AppendDependencies(result, graph);
			File.WriteAllText(file, result.ToString());
		}

		private void AppendProjects(StringBuilder result, DependencyGraph graph)
		{
			var projs = graph.Vertices.ToList();
			projs.Sort(Library.NaturalOrdering);

			foreach (var library in projs)
			{
				if (library is Project)
				{
					var proj = (Project) library;
					result.Append("Project ")
						.Append(proj.ProjectName)
						.Append("\n");
					AppendProperty(result, "Library name", proj.LibraryName);
					if (proj.ProjectPath != null)
						AppendProperty(result, "Project path", proj.ProjectPath);
					if (proj.Guid != null)
						AppendProperty(result, "GUID", proj.Guid.Value);
				}
				else
				{
					result.Append("Library ")
						.Append(library.LibraryName)
						.Append("\n");
				}

				if (library.GroupElement != null)
					AppendProperty(result, "Group", library.GroupElement.Name);

				library.Languages.ForEach(p => AppendProperty(result, "Language", p));

				var projectPath = (library is Project ? ((Project) library).ProjectPath : null);
				library.Paths.Where(p => p != projectPath)
					.ForEach(p => AppendProperty(result, "Path", p));

				result.Append("\n");
			}
		}

		private void AppendDependencies(StringBuilder result, DependencyGraph graph)
		{
			var deps = graph.Edges.ToList();
			deps.Sort(Dependency.NaturalOrdering);

			foreach (var dep in deps)
			{
				result.Append("Dependency: ")
					.Append(dep.Source.Name)
					.Append(" depends on ")
					.Append(dep.Target.Name)
					.Append("\n");

				AppendProperty(result, "Reference type", dep.Type == Dependency.Types.LibraryReference ? "library" : "project");

				if (dep.ReferencedPath != null)
					AppendProperty(result, "Referenced path", dep.ReferencedPath);

				result.Append("\n");
			}
		}

		private void AppendProperty(StringBuilder result, string name, object value)
		{
			result.Append("  - ")
				.Append(name)
				.Append(": ")
				.Append(value)
				.Append("\n");
		}
	}
}