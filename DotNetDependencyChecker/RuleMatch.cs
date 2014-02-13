using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker
{
	public class RuleMatch
	{
		public readonly bool Allowed;
		public readonly string Messsage;
		public readonly ConfigLocation Location;
		public readonly List<Dependency> Dependencies;

		public RuleMatch(bool allowed, string messsage, ConfigLocation location, List<Dependency> dependencies)
		{
			Allowed = allowed;
			Messsage = messsage;
			Location = location;
			Dependencies = dependencies;
		}

		public RuleMatch(bool allowed, string messsage, ConfigLocation location, params Dependency[] dependencies)
			: this(allowed, messsage, location, dependencies.ToList())
		{
		}
	}
}
