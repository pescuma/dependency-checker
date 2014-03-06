using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.utils;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class Project : Library
	{
		private readonly string name;
		public readonly Guid? Guid;

		public override string Name
		{
			get { return name; }
		}

		public string ProjectPath
		{
			get { return Paths.First(); }
		}

		public override List<string> Names
		{
			get
			{
				var result = new List<string>();

				result.Add(Name);

				if (LibraryName != Name)
					result.Add(LibraryName);

				return result;
			}
		}

		public Project(string name, string libraryName, Guid? guid, string projectPath)
			: base(libraryName)
		{
			Argument.ThrowIfNull(name);
			Argument.ThrowIfNull(libraryName);
			Argument.ThrowIfNull(projectPath);

			this.name = name;
			Guid = guid;

			Paths.Add(projectPath);
		}

		protected bool Equals(Project other)
		{
			return base.Equals(other) && string.Equals(Name, other.Name) && string.Equals(ProjectPath, other.ProjectPath);
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
				hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
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
