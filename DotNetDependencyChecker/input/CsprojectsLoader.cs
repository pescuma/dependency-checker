using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dotnetdependencychecker.output;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.input
{
	public class CsprojectsLoader : ProjectLoader
	{
		public void LoadProjects(List<string> paths, DependencyGraphBuilder builder, List<OutputEntry> warnings)
		{
			var csprojsFiles = new HashSet<string>(paths.SelectMany(folder => Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories))
				.Select(Path.GetFullPath));

			var csprojs = csprojsFiles.Select(f => new CsprojReader(f))
				.ToList();

			foreach (var csproj in csprojs)
			{
				var proj = builder.AddProject(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, csproj.Filename);

				foreach (var csref in csproj.ProjectReferences)
					builder.AddProjectReference(proj, csref.Name, null, csref.ProjectGuid, csref.Include, new Location(csproj.Filename, csref.LineNumber));

				foreach (var csref in csproj.References)
					builder.AddDllReference(proj, null, csref.Include.Name, null, csref.HintPath, new Location(csproj.Filename, csref.LineNumber));
			}

			var externalCsprojFiles = csprojs.SelectMany(p => p.ProjectReferences)
				.Select(r => r.Include)
				.Distinct()
				.Where(f => !csprojsFiles.Contains(f));
			foreach (var externalCsprojFile in externalCsprojFiles)
			{
				try
				{
					var csproj = new CsprojReader(externalCsprojFile);
					builder.AddProject(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, csproj.Filename);
				}
				catch (IOException e)
				{
					var msg = new OutputMessage().Append("Failed to load a project outside of input folders: ")
						.Append(externalCsprojFile);
					warnings.Add(new LoadingOutputWarning("External project not found", msg));
				}
			}
		}
	}
}
