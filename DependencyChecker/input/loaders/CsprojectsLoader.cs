using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dependencychecker.output;
using org.pescuma.dependencychecker.rules;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.input.loaders
{
	public class CsprojectsLoader : ProjectLoader
	{
		public void LoadProjects(List<string> paths, DependencyGraphBuilder builder, List<OutputEntry> warnings)
		{
			var csprojsFiles = new HashSet<string>(paths.SelectMany(folder => Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories))
				.Select(Path.GetFullPath));

			var csprojs = csprojsFiles.Select(f => new CsprojReader(f))
				.OrderBy(n => n.Filename, StringComparer.CurrentCultureIgnoreCase)
				.ToList();
			foreach (var csproj in csprojs)
			{
				var proj = builder.AddProject(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, csproj.Filename);

				foreach (var csref in csproj.ProjectReferences)
					builder.AddProjectReference(proj, csref.Name, null, csref.ProjectGuid, csref.Include, new Location(csproj.Filename, csref.LineNumber));

				foreach (var csref in csproj.References)
					builder.AddLibraryReference(proj, null, csref.Include.Name, null, csref.HintPath, new Location(csproj.Filename, csref.LineNumber));
			}

			var externalCsprojFiles = csprojs.SelectMany(p => p.ProjectReferences)
				.Select(r => r.Include)
				.Distinct()
				.Where(f => !csprojsFiles.Contains(f))
				.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase);
			foreach (var externalCsprojFile in externalCsprojFiles)
			{
				try
				{
					var csproj = new CsprojReader(externalCsprojFile);
					builder.AddProject(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, csproj.Filename);
				}
				catch (IOException)
				{
					var msg = new OutputMessage().Append("Failed to load a project outside of input folders: ")
						.Append(externalCsprojFile);
					warnings.Add(new LoadingOutputWarning("External project not found", msg));
				}
			}
		}
	}
}
