using System.Collections.Generic;
using System.Linq;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.presenter.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class SelfDependenciesCommand : BaseCommand
	{
		public override string Name
		{
			get { return "self dependencies"; }
		}

		protected override void InternalHandle(Output result, string args, DependencyGraph graph)
		{
			var projectsFiltered = new HashSet<Library>(FilterLibs(graph, args));

			var projs = graph.Edges.Where(e => e.Source.Equals(e.Target))
				.Select(e => e.Source)
				.Where(projectsFiltered.Contains)
				.ToList();

			if (!projs.Any())
			{
				result.AppendLine("No project/library that depends on itself found");
			}
			else
			{
				projs.ForEach(lib =>
				{
					var proj = lib as Project;
					if (proj != null)
						result.AppendLine("{0} (project path: {1})", GetName(proj), proj.ProjectPath);
					else
						result.AppendLine(GetName(lib));
				});
			}
		}
	}
}
