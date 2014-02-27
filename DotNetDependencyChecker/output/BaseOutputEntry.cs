using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public abstract class BaseOutputEntry : OutputEntry
	{
		public string Type { get; private set; }
		public Severity Severity { get; private set; }
		public OutputMessage Messsage { get; private set; }
		public List<Dependable> Projects { get; private set; }
		public List<Dependency> Dependencies { get; private set; }

		protected BaseOutputEntry(string type, Severity severity, OutputMessage messsage, IEnumerable<Dependable> projects,
			IEnumerable<Dependency> dependencies)
		{
			Type = type;
			Severity = severity;
			Messsage = messsage;

			if (dependencies == null)
				dependencies = messsage.Elements.Select(e => e.Dependendcy)
					.Where(d => d != null)
					.Distinct();

			Dependencies = dependencies.ToList();
			Dependencies.Sort(Dependency.NaturalOrdering);

			if (projects == null)
				projects = messsage.Elements.Select(e => e.Project)
					.Concat(Dependencies.Select(d => d.Source))
					.Concat(Dependencies.Select(d => d.Target))
					.Where(p => p != null)
					.Distinct();

			Projects = projects.ToList();
			Projects.Sort(DependableUtils.NaturalOrdering);
		}

		protected BaseOutputEntry(string type, Severity severity, OutputMessage messsage, IEnumerable<Dependency> dependencies)
			: this(type, severity, messsage, null, dependencies)
		{
		}

		protected BaseOutputEntry(string type, Severity severity, OutputMessage messsage)
			: this(type, severity, messsage, null, null)
		{
		}
	}
}
