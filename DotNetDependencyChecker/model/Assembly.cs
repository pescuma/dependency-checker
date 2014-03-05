using System;
using System.Collections.Generic;
using System.Text;
using org.pescuma.dotnetdependencychecker.utils;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class Assembly
	{
		public static Comparison<Assembly> NaturalOrdering =
			(p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.CurrentCultureIgnoreCase);

		public readonly string AssemblyName;
		public readonly HashSet<string> Paths = new HashSet<string>();
		public GroupElement GroupElement;

		public Assembly(string assemblyName)
		{
			Argument.ThrowIfNull(assemblyName);

			AssemblyName = assemblyName;
		}

		public virtual string Name
		{
			get { return AssemblyName; }
		}

		public virtual List<string> Names
		{
			get { return AssemblyName.AsList(); }
		}

		protected bool Equals(Assembly other)
		{
			return string.Equals(AssemblyName, other.AssemblyName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Assembly) obj);
		}

		public override int GetHashCode()
		{
			return (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
		}

		public override string ToString()
		{
			var result = new StringBuilder();

			result.Append(AssemblyName)
				.Append("[")
				.Append("Paths: ")
				.Append(string.Join(", ", Paths))
				.Append("]");

			return result.ToString();
		}
	}
}
