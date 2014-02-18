using System.Collections.Generic;
using System.Text;
using org.pescuma.dotnetdependencychecker.utils;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class Assembly : Dependable
	{
		public readonly string AssemblyName;
		public readonly HashSet<string> Paths = new HashSet<string>();
		public Group Group;

		public Assembly(string assemblyName)
		{
			Argument.ThrowIfNull(assemblyName);

			AssemblyName = assemblyName;
		}

		IEnumerable<string> Dependable.Names
		{
			get { return AssemblyName.AsList(); }
		}

		IEnumerable<string> Dependable.Paths
		{
			get { return Paths; }
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
