using System;
using org.pescuma.dotnetdependencychecker.utils;
using QuickGraph;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class Dependency : Edge<Assembly>
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
		public readonly string DLLHintPath;

		public enum Types
		{
			ProjectReference,
			DllReference
		}

		public static Dependency WithProject(Assembly source, Assembly target, Location location)
		{
			return new Dependency(source, target, Types.ProjectReference, location, null);
		}

		public static Dependency WithAssembly(Assembly source, Assembly target, Location location, string dllPath)
		{
			return new Dependency(source, target, Types.DllReference, location, dllPath);
		}

		private Dependency(Assembly source, Assembly target, Types type, Location location, string dllHintPath)
			: base(source, target)
		{
			Argument.ThrowIfNull(source);
			Argument.ThrowIfNull(location);

			Type = type;
			Location = location;
			DLLHintPath = dllHintPath;
		}

		public Dependency WithTarget(Assembly otherTarget)
		{
			if (Equals(otherTarget, Target))
				return this;

			return new Dependency(Source, otherTarget, Type, Location, DLLHintPath);
		}

		public Dependency WithSource(Assembly otherSource)
		{
			if (Equals(otherSource, Source))
				return this;

			return new Dependency(otherSource, Target, Type, Location, DLLHintPath);
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
