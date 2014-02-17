using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class RuleMatch
	{
		public readonly bool Allowed;
		public readonly Severity Severity;
		public readonly OutputMessage Messsage;
		public readonly ConfigLocation Location;
		public readonly List<Dependable> Projects;
		public readonly List<Dependency> Dependencies;

		public RuleMatch(bool allowed, Severity severity, OutputMessage messsage, ConfigLocation location, params Dependency[] dependencies)
			: this(allowed, severity, messsage, location, //
				dependencies.Select(d => d.Source)
					.Concat(dependencies.Select(d => d.Target))
					.Where(p => p != null)
					.Distinct(), //
				dependencies)
		{
		}

		public RuleMatch(bool allowed, Severity severity, OutputMessage messsage, ConfigLocation location, IEnumerable<Dependable> projects,
			IEnumerable<Dependency> dependencies)
		{
			Allowed = allowed;
			Severity = severity;
			Messsage = messsage;
			Location = location;
			Projects = new List<Dependable>(projects ?? Enumerable.Empty<Dependable>());
			Dependencies = new List<Dependency>(dependencies ?? Enumerable.Empty<Dependency>());

			Projects.Sort(DependableUtils.NaturalOrdering);
			Dependencies.Sort(Dependency.NaturalOrdering);
		}
	}
}
