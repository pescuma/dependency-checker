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
		public readonly List<Dependency> Dependencies;

		public RuleMatch(bool allowed, Severity severity, string messsage, ConfigLocation location, List<Dependency> dependencies)
		{
			Allowed = allowed;
			this.Severity = severity;
			Messsage = messsage;
			Location = location;
			Dependencies = dependencies;
		}

		public RuleMatch(bool allowed, Severity severity, string messsage, ConfigLocation location, params Dependency[] dependencies)
			: this(allowed, severity, messsage, location, dependencies.ToList())
		{
		}
	}
}
