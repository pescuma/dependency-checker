using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.utils;

namespace org.pescuma.dotnetdependencychecker
{
	public class BuildScript
	{
		public readonly List<BuildThread> ParallelThreads = new List<BuildThread>();
	}

	public class BuildThread
	{
		public readonly List<BuildStep> Steps = new List<BuildStep>();
	}

	public interface BuildStep
	{
	}

	public class BuildProject : BuildStep
	{
		public readonly Project Project;

		public BuildProject(Project project)
		{
			Argument.ThrowIfNull(project);

			Project = project;
		}

		protected bool Equals(BuildProject other)
		{
			return Equals(Project, other.Project);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((BuildProject) obj);
		}

		public override int GetHashCode()
		{
			return (Project != null ? Project.GetHashCode() : 0);
		}
	}

	public class CopyProjectOutput : BuildStep
	{
		public readonly Project Project;
		public readonly string DllPath;

		public CopyProjectOutput(Project project, string dllPath)
		{
			Argument.ThrowIfNull(project);
			Argument.ThrowIfNull(dllPath);

			Project = project;
			DllPath = dllPath;
		}

		protected bool Equals(CopyProjectOutput other)
		{
			return Equals(Project, other.Project) && string.Equals(DllPath, other.DllPath);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((CopyProjectOutput) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Project != null ? Project.GetHashCode() : 0) * 397) ^ (DllPath != null ? DllPath.GetHashCode() : 0);
			}
		}
	}

	public class MaterializeDll : BuildStep
	{
		public readonly Assembly Assembly;
		public readonly string DllPath;

		public MaterializeDll(Assembly assembly, string dllPath)
		{
			Argument.ThrowIfNull(assembly);
			Argument.ThrowIfNull(dllPath);

			Assembly = assembly;
			DllPath = dllPath;
		}

		protected bool Equals(MaterializeDll other)
		{
			return Equals(Assembly, other.Assembly) && string.Equals(DllPath, other.DllPath);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((MaterializeDll) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Assembly != null ? Assembly.GetHashCode() : 0) * 397) ^ (DllPath != null ? DllPath.GetHashCode() : 0);
			}
		}
	}

	public class BuildCircularDependentProjects : BuildStep
	{
		public readonly CircularDependencyGroup Projects;
		public readonly List<BuildStep> Steps = new List<BuildStep>();

		public BuildCircularDependentProjects(CircularDependencyGroup projects)
		{
			Argument.ThrowIfNull(projects);

			Projects = projects;
		}

		protected bool Equals(BuildCircularDependentProjects other)
		{
			return Equals(Projects, other.Projects) && Equals(Steps, other.Steps);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((BuildCircularDependentProjects) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Projects != null ? Projects.GetHashCode() : 0) * 397) ^ (Steps != null ? Steps.GetHashCode() : 0);
			}
		}
	}
}
