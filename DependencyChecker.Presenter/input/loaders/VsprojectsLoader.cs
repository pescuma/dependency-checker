﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.presenter.rules;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.input.loaders
{
	public class VsProjectsLoader : ProjectLoader
	{
		public void LoadProjects(List<string> paths, DependencyGraphBuilder builder, List<OutputEntry> warnings)
		{
			LoadProjects(paths, builder, warnings, "*.csproj", "C#");
			LoadProjects(paths, builder, warnings, "*.vbproj", "Visual Basic");
			LoadProjects(paths, builder, warnings, "*.fsproj", "F#");
		}

		private static void LoadProjects(List<string> paths, DependencyGraphBuilder builder, List<OutputEntry> warnings, string filenamePattern,
			params string[] defaultLanguage)
		{
			var csprojsFiles = new HashSet<string>(paths
					.SelectMany(folder => Directory.GetFiles(folder, filenamePattern, SearchOption.AllDirectories))
					.Select(Path.GetFullPath));

			List<VSProjReader> csprojs = csprojsFiles.Select(f => new VSProjReader(f))
					.OrderBy(n => n.Filename, StringComparer.CurrentCultureIgnoreCase)
					.ToList();
			foreach (VSProjReader csproj in csprojs)
			{
				object proj = builder.AddProject(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, csproj.Filename, csproj.OutputFiles,
					csproj.DocumentationFiles, defaultLanguage);

				foreach (VSProjReader.ProjectReference csref in csproj.ProjectReferences)
					builder.AddProjectReference(proj, csref.Name, null, csref.ProjectGuid, csref.Include, new Location(csproj.Filename, csref.LineNumber),
						defaultLanguage);

				foreach (VSProjReader.Reference csref in csproj.References)
				{
					IEnumerable<string> language;
					if (csref.HintPath == null && csref.Include.GetPublicKey() == null)
							// A system lib
						language = defaultLanguage;
					else
						language = null;

					builder.AddLibraryReference(proj, null, csref.Include.Name, null, csref.HintPath, new Location(csproj.Filename, csref.LineNumber),
						language);
				}

				foreach (VSProjReader.COMReference csref in csproj.COMReferences)
					builder.AddLibraryReference(proj, null, csref.Include, csref.Guid, null, new Location(csproj.Filename, csref.LineNumber), null);
			}

			IOrderedEnumerable<string> externalCsprojFiles = csprojs.SelectMany(p => p.ProjectReferences)
					.Select(r => r.Include)
					.Distinct()
					.Where(f => !csprojsFiles.Contains(f))
					.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase);
			foreach (string externalCsprojFile in externalCsprojFiles)
			{
				if (paths.Any(p => externalCsprojFile.StartsWith(p + Path.DirectorySeparatorChar)))
				{
					OutputMessage msg = new OutputMessage().Append("Failed to load a referenced project: ")
							.Append(externalCsprojFile);
					warnings.Add(new LoadingOutputEntry("Referenced project not found", msg));
					continue;
				}

				try
				{
					var csproj = new VSProjReader(externalCsprojFile);
					builder.AddProject(csproj.Name, csproj.AssemblyName, csproj.ProjectGuid, csproj.Filename, csproj.OutputFiles,
						csproj.DocumentationFiles, defaultLanguage);
				}
				catch (IOException)
				{
					OutputMessage msg = new OutputMessage().Append("Failed to load a project outside of input folders: ")
							.Append(externalCsprojFile);
					warnings.Add(new LoadingOutputEntry("External project not found", msg));
				}
			}
		}
	}
}
