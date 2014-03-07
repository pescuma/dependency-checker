using System;
using System.Text;
using org.pescuma.dependencychecker.utils;

namespace org.pescuma.dependencychecker.model
{
	public class Project : Library
	{
		public readonly string ProjectName;
		public readonly string ProjectPath;
		public readonly Guid? Guid;

		public override string Name
		{
			get { return ProjectName; }
		}

		public Project(string projectName, string libraryName, Guid? guid, string projectPath)
			: base(libraryName)
		{
			Argument.ThrowIfNull(projectName);
			Argument.ThrowIfNull(libraryName);

			ProjectName = projectName;
			ProjectPath = projectPath;
			Guid = guid;

			Names.Add(projectName);

			if (projectPath != null)
				Paths.Add(projectPath);
		}

		protected bool Equals(Project other)
		{
			return base.Equals(other) && string.Equals(ProjectName, other.ProjectName) && string.Equals(ProjectPath, other.ProjectPath);
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
				var hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (ProjectName != null ? ProjectName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ProjectPath != null ? ProjectPath.GetHashCode() : 0);
				return hashCode;
			}
		}

		public override string ToString()
		{
			var result = new StringBuilder();

			result.Append(Name)
				.Append("[");

			if (LibraryName != null)
				result.Append(LibraryName)
					.Append(", ");

			if (Guid != null)
				result.Append(Guid)
					.Append(", ");

			if (ProjectPath != null)
				result.Append(ProjectPath)
					.Append(", ");

			result.Append("Paths: ")
				.Append(string.Join(", ", Paths));

			result.Append("]");

			return result.ToString();
		}
	}
}
