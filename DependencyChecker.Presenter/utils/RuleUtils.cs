using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.output;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.presenter.utils
{
	public class RuleUtils
	{
		public static void ReportUnusedConfig(List<OutputEntry> warnings, string type, IEnumerable<ConfigLocation> all,
			HashSet<ConfigLocation> used)
		{
			all.Where(l => !used.Contains(l))
				.OrderBy(l => l.LineNum)
				.ForEach(l => warnings.Add(new UnusedConfigOutputEntry(type, l)));
		}
	}
}
