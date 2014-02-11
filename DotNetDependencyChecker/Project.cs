namespace org.pescuma.dotnetdependencychecker
{
	public class Project
	{
		public readonly string Name;

		public Project(string name)
		{
			this.Name = name;
		}

		protected bool Equals(Project other)
		{
			return string.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((Project) obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
