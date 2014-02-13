using QuickGraph;

namespace org.pescuma.dotnetdependencychecker
{
	public class Dependency : Edge<Project>
	{
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
