using System;
using org.pescuma.dependencychecker.utils;
using QuickGraph;

namespace org.pescuma.dependencychecker.model
{
	public class Dependency : Edge<Library>
	{
		public static Comparison<Dependency> NaturalOrdering = (d1, d2) =>
		{
			var comp = Library.NaturalOrdering(d1.Source, d2.Source);
			if (comp != 0)
				return comp;

			comp = Library.NaturalOrdering(d1.Target, d2.Target);
			if (comp != 0)
				return comp;

			return d1.Type - d2.Type;
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
