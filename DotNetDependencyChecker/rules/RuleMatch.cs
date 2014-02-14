using System.Collections.Generic;
using System.Linq;
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

		public RuleMatch(bool allowed, Severity severity, string messsage, ConfigLocation location, params Dependency[] dependencies)
			: this(allowed, severity, messsage, location, //
				dependencies.Select(d => d.Source)
					.Concat(dependencies.Select(d => d.Target))
					.Where(p => p != null)
					.Distinct(), //
				dependencies)
		{
		}

		public RuleMatch(bool allowed, Severity severity, string messsage, ConfigLocation location, IEnumerable<Project> projects,
			IEnumerable<Dependency> dependencies)
		{
			Allowed = allowed;
			Severity = severity;
			Messsage = messsage;
			Location = location;
			Projects = new List<Project>(projects ?? Enumerable.Empty<Project>());
			Dependencies = new List<Dependency>(dependencies ?? Enumerable.Empty<Dependency>());

			Projects.Sort(Project.NaturalOrdering);
			Dependencies.Sort(Dependency.NaturalOrdering);
		}
	}
}
