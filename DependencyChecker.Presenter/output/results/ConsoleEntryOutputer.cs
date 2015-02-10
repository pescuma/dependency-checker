using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.rules;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.output.results
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

		private static string Simplify(string text)
		{
			return Regex.Replace(text, @"\s+", " ");
		}

		public static string ToConsole(OutputEntry entry, bool verbose)
		{
			var result = new StringBuilder();

			result.AppendPrefix(entry)
				.Append(ToConsole(entry.Messsage));

			if (!verbose)
			{
				entry.ProcessedFields.Where(f => f.Matched)
					.Where(f => !f.Field.EndsWith("Library Name"))
					.ForEach(f =>
					{
						result.AppendLine()
							.AppendPrefix(entry)
							.Append("   ")
							.Append(f.Field)
							.Append(": ")
							.Append(f.Value);
					});
			}
			else
			{
				if (entry is RuleOutputEntry)
				{
					result.AppendLine()
						.AppendPrefix(entry)
						.Append("   ")
						.Append(Simplify(((RuleOutputEntry) entry).Rule.Location.LineText));
				}

				entry.ProcessedFields.Where(f => f.Matched)
					.ForEach(f =>
					{
						result.AppendLine()
							.AppendPrefix(entry)
							.Append("   ")
							.Append(f.Field)
							.Append(": ")
							.Append(f.Value);
					});

				if (entry.Projects.Any())
				{
					result.AppendLine()
						.AppendPrefix(entry)
						.Append("   Projects affected:");
					entry.Projects.ForEach(p => result.AppendLine()
						.AppendPrefix(entry)
						.Append("      - ")
						.Append(ToConsole(p, OutputMessage.ProjInfo.NameAndPath)));
				}

				if (entry.Dependencies.Any())
				{
					result.AppendLine()
						.AppendPrefix(entry)
						.Append("   Dependencies affected:");
					entry.Dependencies.ForEach(d => result.AppendLine()
						.AppendPrefix(entry)
						.Append("      - ")
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
					return string.Join(" or ", proj.SortedNames);
				}
				case OutputMessage.ProjInfo.NameAndGroup:
				{
					var result = ToConsole(proj, OutputMessage.ProjInfo.Name);

					var group = proj.GroupElement;
					if (group != null)
						result = string.Format("{0} (in group {1})", result, group.Name);

					return result;
				}
				case OutputMessage.ProjInfo.NameAndProjectPath:
				{
					return string.Format("{0} ({1})", ToConsole(proj, OutputMessage.ProjInfo.Name), ToConsole(proj, OutputMessage.ProjInfo.ProjectPath));
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
				case OutputMessage.ProjInfo.ProjectPath:
				{
					if (proj is Project)
						return ((Project) proj).ProjectPath;
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
						ToConsole(dep, OutputMessage.DepInfo.Line), ToConsole(dep.Source, OutputMessage.ProjInfo.NameAndProjectPath),
						ToConsole(dep.Target, OutputMessage.ProjInfo.NameAndPath));
				}
				default:
					throw new InvalidDataException();
			}
		}
	}

	internal static class StringBuilderExtensions
	{
		public static StringBuilder AppendPrefix(this StringBuilder result, OutputEntry entry)
		{
			result.Append("[");
			if (entry is DependencyRuleMatch && ((DependencyRuleMatch) entry).Allowed)
				result.Append("ALLOWED");
			else
				result.Append(entry.Severity.ToString()
					.ToUpper());
			result.Append("] ");

			return result;
		}
	}
}
