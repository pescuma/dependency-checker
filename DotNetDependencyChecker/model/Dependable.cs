using System;
using System.Collections.Generic;
using System.Linq;

namespace org.pescuma.dotnetdependencychecker.model
{
	public interface Dependable
	{
		IEnumerable<string> Names { get; }
		IEnumerable<string> Paths { get; }
	}

	public static class DependableUtils
	{
		public static Comparison<Dependable> NaturalOrdering =
			(p1, p2) => String.Compare(p1.Names.First(), p2.Names.First(), StringComparison.CurrentCultureIgnoreCase);
	}
}
