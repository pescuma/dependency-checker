using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class RuleMatch
	{
		public readonly bool Allowed;
		public readonly Severity Severity;
		public readonly string Messsage;
		public readonly ConfigLocation Location;
		public readonly List<Project> Projects;
		public readonly List<Dependency> Dependencies;

		public RuleMatch(bool allowed, Severity severity, string messsage, ConfigLocation location, List<Project> projects,
			List<Dependency> dependencies)
		{
			Allowed = allowed;
			Severity = severity;
			Messsage = messsage;
			Location = location;
			Projects = projects ?? new List<Project>();
			Dependencies = dependencies ?? new List<Dependency>();

			Projects.Sort(Project.NaturalOrdering);
			Dependencies.Sort(Dependency.NaturalOrdering);
		}
	}
}
