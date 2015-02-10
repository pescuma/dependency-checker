using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter.output
{
	public abstract class BaseOutputEntry : OutputEntry
	{
		public string Type { get; private set; }
		public Severity Severity { get; private set; }
		public OutputMessage Messsage { get; private set; }
		public List<Library> Projects { get; private set; }
		public List<Dependency> Dependencies { get; private set; }
		public List<ProcessedField> ProcessedFields { get; private set; }

		protected BaseOutputEntry(string type, Severity severity, OutputMessage messsage, IEnumerable<Library> projects = null,
			IEnumerable<Dependency> dependencies = null, IEnumerable<ProcessedField> processedFields = null)
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
			Projects.Sort(Library.NaturalOrdering);

			ProcessedFields = processedFields != null ? processedFields.ToList() : new List<ProcessedField>();
		}

		protected BaseOutputEntry(string type, Severity severity, OutputMessage messsage, IEnumerable<Dependency> dependencies)
			: this(type, severity, messsage, null, dependencies)
		{
		}
	}
}
