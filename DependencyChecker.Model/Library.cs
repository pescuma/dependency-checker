using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.model
{
	public class Library
	{
// ReSharper disable once InconsistentNaming
		public static readonly Comparison<Library> NaturalOrdering = (p1, p2) =>
		{
			var cmp = String.Compare(p1.Name, p2.Name, StringComparison.CurrentCultureIgnoreCase);
			if (cmp != 0)
				return cmp;

			cmp = String.Compare(p1.LibraryName, p2.LibraryName, StringComparison.CurrentCultureIgnoreCase);
			if (cmp != 0)
				return cmp;

			cmp = String.Compare(p1.Paths.FirstOrDefault() ?? "", p2.Paths.FirstOrDefault() ?? "", StringComparison.CurrentCultureIgnoreCase);
			if (cmp != 0)
				return cmp;

			var proj1 = (p1 is Project ? 1 : 0);
			var proj2 = (p2 is Project ? 1 : 0);
			return proj1 - proj2;
		};

		public readonly string LibraryName;
		public readonly ISet<string> Paths = new HashSet<string>();
		public GroupElement GroupElement;
		public readonly ISet<string> Names = new HashSet<string>();
		public readonly ISet<string> LibraryNames = new HashSet<string>();
		public readonly ISet<string> Languages = new HashSet<string>();
		public bool IsLocal;

		public Library(string libraryName, IEnumerable<string> languages)
		{
			Argument.ThrowIfNull(libraryName);

			LibraryName = libraryName;

			Names.Add(libraryName);
			LibraryNames.Add(libraryName);

			if (languages != null)
				Languages.AddRange(languages);
		}

		public virtual string Name
		{
			get { return LibraryName; }
		}

		public List<string> SortedNames
		{
			get
			{
				var result = new List<string>();
				result.Add(Name);
				if (LibraryName != Name)
					result.Add(LibraryName);
				result.AddRange(Names.Where(n => !result.Contains(n))
					.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase));
				return result;
			}
		}

		protected bool Equals(Library other)
		{
			return string.Equals(LibraryName, other.LibraryName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Library) obj);
		}

		public override int GetHashCode()
		{
			return (LibraryName != null ? LibraryName.GetHashCode() : 0);
		}

		public override string ToString()
		{
			var result = new StringBuilder();

			result.Append(LibraryName)
				.Append("[")
				.Append("Paths: ")
				.Append(string.Join(", ", Paths))
				.Append("]");

			return result.ToString();
		}
	}
}
