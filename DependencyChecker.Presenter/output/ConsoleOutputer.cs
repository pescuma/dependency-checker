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
				var g1 = e1.Key;
				var g2 = e2.Key;

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

				var projs = g.ToList();
				projs.Sort(Library.NaturalOrdering);
				projs.ForEach(p => result.AppendLine(string.Join(" or ", p.SortedNames)));

				result.DecreaseIndent();
				result.AppendLine();
			});

			return result.ToString();
		}
	}
}
