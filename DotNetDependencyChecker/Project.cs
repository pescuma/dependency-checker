using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker
{
	public class Project
	{
		public readonly string Name;
		public readonly string LocalPath;
		public readonly List<string> Paths = new List<string>();

		public Project(string name, string localPath)
		{
			Name = name;
			LocalPath = localPath;

			if (localPath != null)
				Paths.Add(LocalPath);
		}

		public bool IsLocal
		{
			get { return LocalPath != null; }
		}

		public string ToGui()
		{
			if (LocalPath != null)
				return string.Format("{0}({1})", Name, LocalPath);
			else
				return Name;
		}

		public override string ToString()
		{
			return Name;
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
	}
}
