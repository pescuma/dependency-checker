using System;
using System.Collections.Generic;
using System.Text;

namespace org.pescuma.dotnetdependencychecker
{
	public class Project
	{
		public static Comparison<Project> NaturalOrdering =
			(p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.CurrentCultureIgnoreCase);

		public readonly string Name;
		public readonly string AssemblyName;
		public readonly Guid? Guid;
		public readonly string CsprojPath;
		public readonly HashSet<string> Paths = new HashSet<string>();

		public Project(string name, string assemblyName, Guid? guid, string csprojPath)
		{
			Name = name;
			AssemblyName = assemblyName;
			Guid = guid;
			CsprojPath = csprojPath;

			if (csprojPath != null)
				Paths.Add(csprojPath);
		}

		protected bool Equals(Project other)
		{
			return string.Equals(Name, other.Name) && string.Equals(AssemblyName, other.AssemblyName) && Guid.Equals(other.Guid)
			       && string.Equals(CsprojPath, other.CsprojPath);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Project) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Name != null ? Name.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Guid.GetHashCode();
				hashCode = (hashCode * 397) ^ (CsprojPath != null ? CsprojPath.GetHashCode() : 0);
				return hashCode;
			}
		}

		public override string ToString()
		{
			var result = new StringBuilder();

			result.Append(Name)
				.Append("[");

			if (AssemblyName != null)
				result.Append(AssemblyName)
					.Append(", ");

			if (Guid != null)
				result.Append(Guid)
					.Append(", ");

			if (CsprojPath != null)
				result.Append(CsprojPath)
					.Append(", ");

			result.Append("Paths: ")
				.Append(string.Join(", ", Paths));

			return result.ToString();
		}

		// Helpers for messages

		public string GetNameAndPath()
		{
			if (CsprojPath != null)
				return string.Format("{0} ({1})", Name, CsprojPath);
			else
				return Name;
		}

		public string GetCsprojOrFullID()
		{
			var msg = new StringBuilder();

			if (CsprojPath != null)
			{
				msg.Append(CsprojPath);
			}
			else
			{
				msg.Append(Name);

				if (AssemblyName != Name)
					msg.Append(", Assembly name: ")
						.Append(AssemblyName);

				if (Guid != null)
					msg.Append(", GUID: ")
						.Append(Guid);
			}

			return msg.ToString();
		}
	}
}
