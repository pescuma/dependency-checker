using System;
using QuickGraph;

namespace org.pescuma.dotnetdependencychecker
{
	public class Dependency : Edge<Project>
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

		public enum Types
		{
			ProjectReference,
			DllReference
		}

		public Dependency(Project source, Project target, Types type, Location location)
			: base(source, target)
		{
			Type = type;
			Location = location;
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
			return string.Format("{0} => {1} ({2})", Source.Name, Target.Name, Type);
		}
	}
}
