using System;
using System.Collections.Generic;

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
		public readonly bool IsLocal;
		public readonly HashSet<string> Paths = new HashSet<string>();

		public Project(string name, string assemblyName, Guid? guid, string csprojPath, bool isLocal, params string[] paths)
		{
			Name = name;
			AssemblyName = assemblyName;
			Guid = guid;
			CsprojPath = csprojPath;
			IsLocal = isLocal;

			if (csprojPath != null)
				Paths.Add(csprojPath);

			paths.ForEach(p => Paths.Add(p));
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
			return string.Format("{0}[{1},{2}, csproj: {3}, {4}, Paths: {5}", Name, AssemblyName, Guid, CsprojPath, IsLocal ? "Local" : "Not local",
				Paths);
		}
	}
}
