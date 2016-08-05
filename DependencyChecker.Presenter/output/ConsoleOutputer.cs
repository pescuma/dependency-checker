using System;
using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.utils;

namespace org.pescuma.dependencychecker.presenter.output
{
	public class ConsoleOutputer
	{
		public static string GroupsToConsole(Output result, IEnumerable<Library> projects)
		{
			var groups = projects.GroupBy(p => p.GroupElement)
				.ToList();

			groups.Sort((e1, e2) =>
			{
				GroupElement g1 = e1.Key;
				GroupElement g2 = e2.Key;

				if (Equals(g1, g2))
					return 0;

				if (g1 == null)
					return 1;

				if (g2 == null)
					return -1;

				return string.Compare(g1.Name, g2.Name, StringComparison.CurrentCultureIgnoreCase);
			});

			groups.ForEach(g =>
			{
				result.AppendLine((g.Key != null ? g.Key.Name : "Without a group") + ":");

				result.IncreaseIndent();

				List<Library> projs = g.ToList();
				projs.Sort(Library.NaturalOrdering);
				projs.ForEach(p => result.AppendLine(ProjectGlance(p)));

				result.DecreaseIndent();
				result.AppendLine();
			});

			return result.ToString();
		}

		public static string ProjectGlance(Library p)
		{
			return string.Join(" or ", p.SortedNames) + " (" + (p is Project ? "project" : "library") + (p.IsLocal ? " local" : "") + ")";
		}
	}
}
