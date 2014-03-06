using System;
using org.pescuma.dotnetdependencychecker.utils;
using QuickGraph;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class Dependency : Edge<Library>
	{
		public static Comparison<Dependency> NaturalOrdering = (d1, d2) =>
		{
			var comp = string.Compare(d1.Source.Name, d2.Source.Name, StringComparison.CurrentCultureIgnoreCase);
			if (comp != 0)
				return comp;

			return string.Compare(d1.Target.Name, d2.Target.Name, StringComparison.CurrentCultureIgnoreCase);
		};

		public readonly Types Type;
		public readonly Location Location;
		public readonly string ReferencedPath;

		public enum Types
		{
			ProjectReference,
			LibraryReference
		}

		public static Dependency WithProject(Library source, Library target, Location location)
		{
			return new Dependency(source, target, Types.ProjectReference, location, null);
		}

		public static Dependency WithLibrary(Library source, Library target, Location location, string referencedPath)
		{
			return new Dependency(source, target, Types.LibraryReference, location, referencedPath);
		}

		private Dependency(Library source, Library target, Types type, Location location, string referencedPath)
			: base(source, target)
		{
			Argument.ThrowIfNull(source);
			Argument.ThrowIfNull(location);

			Type = type;
			Location = location;
			ReferencedPath = referencedPath;
		}

		public Dependency WithTarget(Library otherTarget)
		{
			if (Equals(otherTarget, Target))
				return this;

			return new Dependency(Source, otherTarget, Type, Location, ReferencedPath);
		}

		public Dependency WithSource(Library otherSource)
		{
			if (Equals(otherSource, Source))
				return this;

			return new Dependency(otherSource, Target, Type, Location, ReferencedPath);
		}

		protected bool Equals(Dependency other)
		{
			return Equals(Source, other.Source) && Equals(Target, other.Target) && Type == other.Type;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Dependency) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Source != null ? Source.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Target != null ? Target.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (int) Type;
				return hashCode;
			}
		}

		public override string ToString()
		{
			return string.Format("{0} -> {1} ({2})", Source.Name, Target.Name, Type);
		}
	}
}
