using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public abstract class BaseOutputEntry : OutputEntry
	{
		public Severity Severity { get; private set; }
		public OutputMessage Messsage { get; private set; }
		public List<Dependable> Projects { get; private set; }
		public List<Dependency> Dependencies { get; private set; }

		protected BaseOutputEntry(Severity severity, OutputMessage messsage, IEnumerable<Dependable> projects,
			IEnumerable<Dependency> dependencies)
		{
			Severity = severity;
			Messsage = messsage;
			Projects = new List<Dependable>(projects ?? Enumerable.Empty<Dependable>());
			Dependencies = new List<Dependency>(dependencies ?? Enumerable.Empty<Dependency>());

			Projects.Sort(DependableUtils.NaturalOrdering);
			Dependencies.Sort(Dependency.NaturalOrdering);
		}

		protected BaseOutputEntry(Severity severity, OutputMessage messsage, Dependency[] dependencies)
			: this(severity, messsage, GetProjects(dependencies), //
				dependencies)
		{
		}

		protected BaseOutputEntry(Severity severity, OutputMessage messsage)
			: this(severity, messsage, //
				messsage.Elements.Select(e => e.Project)
					.Where(p => p != null)
					.Concat(GetProjects(messsage.Elements.Select(e => e.Dependendcy)))
					.Distinct(), //
				messsage.Elements.Select(e => e.Dependendcy)
					.Where(d => d != null)
					.Distinct())
		{
		}

		private static IEnumerable<Dependable> GetProjects(IEnumerable<Dependency> dependencies)
		{
			var ds = dependencies as IList<Dependency> ?? dependencies.ToList();
			return ds.Select(d => d.Source)
				.Concat(ds.Select(d => d.Target))
				.Where(p => p != null)
				.Distinct();
		}
	}
}
