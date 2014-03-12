using System;
using System.Collections.Generic;
using System.Linq;

namespace org.pescuma.dependencychecker
{
	public class Library
	{
		public static Comparison<Library> NaturalOrdering = (p1, p2) =>
		{
			return String.Compare(p1.Names.First(), p2.Names.First(), StringComparison.CurrentCultureIgnoreCase);
		};

		public string Type;
		public string Group;
		public readonly List<string> Names = new List<string>();
		public readonly List<string> Paths = new List<string>();
		public readonly List<string> Languages = new List<string>();
	}
}
