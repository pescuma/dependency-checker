using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output.results
{
	public class ConsoleEntryOutputer : EntryOutputer
	{
		private readonly bool verbose;

		public ConsoleEntryOutputer(bool verbose)
		{
			this.verbose = verbose;
		}

		public void Output(List<OutputEntry> entries)
		{
			entries.ForEach(e =>
			{
				Console.WriteLine(ToConsole(e, verbose));
				Console.WriteLine();
			});
		}

		public static string ToConsole(OutputEntry entry, bool verbose)
		{
			var result = new StringBuilder();

			result.Append("[")
				.Append(entry.Severity.ToString()
					.ToUpper())
				.Append("] ");
			result.Append(ToConsole(entry.Messsage));

			if (verbose)
			{
				if (entry.Projects.Any())
				{
					result.Append("\nProjects affected:");
					entry.Projects.ForEach(p => result.Append("\n  - ")
						.Append(ToConsole(p, OutputMessage.ProjInfo.NameAndPath)));
				}

				if (entry.Dependencies.Any())
				{
					result.Append("\nDependencies affected:");
					entry.Dependencies.ForEach(d => result.Append("\n  - ")
						.Append(ToConsole(d, OutputMessage.DepInfo.FullDescription)));
				}
			}

			return result.ToString();
		}

		public static string ToConsole(OutputMessage messsage)
		{
			return string.Join("", messsage.Elements.Select(e =>
			{
				if (e.Text != null)
					return e.Text;

				else if (e.Project != null)
					return ToConsole(e.Project, e.ProjInfo);

				else if (e.Dependendcy != null)
					return ToConsole(e.Dependendcy, e.DepInfo);

				else
					throw new InvalidDataException();
			}));
		}

		private static string ToConsole(Library proj, OutputMessage.ProjInfo info)
		{
			switch (info)
			{
				case OutputMessage.ProjInfo.Name:
				{
					return string.Join(" or ", proj.Names);
				}
				case OutputMessage.ProjInfo.NameAndGroup:
				{
					var result = ToConsole(proj, OutputMessage.ProjInfo.Name);

					var group = proj.GroupElement;
					if (group != null)
						result = string.Format("{0} (in group {1})", result, group.Name);

					return result;
				}
				case OutputMessage.ProjInfo.NameAndCsproj:
				{
					return string.Format("{0} ({1})", ToConsole(proj, OutputMessage.ProjInfo.Name), ToConsole(proj, OutputMessage.ProjInfo.Csproj));
				}
				case OutputMessage.ProjInfo.NameAndPath:
				{
					if (proj.Paths.Any())
						return string.Format("{0} ({1})", ToConsole(proj, OutputMessage.ProjInfo.Name), ToConsole(proj, OutputMessage.ProjInfo.Path));
					else
						return ToConsole(proj, OutputMessage.ProjInfo.Name);
				}
				case OutputMessage.ProjInfo.Path:
				{
					if (proj.Paths.Any())
						return string.Join(" or ", proj.Paths);
					else
						return ToConsole(proj, OutputMessage.ProjInfo.Name);
				}
				case OutputMessage.ProjInfo.Csproj:
				{
					if (proj is Project)
						return ((Project) proj).CsprojPath;
					else
						throw new InvalidDataException();
				}
				default:
					throw new InvalidDataException();
			}
		}

		private static string ToConsole(Dependency dep, OutputMessage.DepInfo info)
		{
			switch (info)
			{
				case OutputMessage.DepInfo.Type:
				{
					switch (dep.Type)
					{
						case Dependency.Types.LibraryReference:
							return "library reference";
						case Dependency.Types.ProjectReference:
							return "project reference";
						default:
							throw new InvalidDataException();
					}
				}
				case OutputMessage.DepInfo.Line:
				{
					return "line " + dep.Location.Line;
				}
				case OutputMessage.DepInfo.FullDescription:
				{
					return string.Format("{0} in {1} of {2} pointing to {3}", ToConsole(dep, OutputMessage.DepInfo.Type),
						ToConsole(dep, OutputMessage.DepInfo.Line), ToConsole(dep.Source, OutputMessage.ProjInfo.NameAndCsproj),
						ToConsole(dep.Target, OutputMessage.ProjInfo.NameAndPath));
				}
				default:
					throw new InvalidDataException();
			}
		}
	}
}
