using org.pescuma.dependencychecker.model;
using org.pescuma.dependencyconsole.utils;

namespace org.pescuma.dependencyconsole.commands
{
	internal class ReferencedByCommand : BaseReferencesCommand
	{
		public override string Name
		{
			get { return "referenced by"; }
		}

		protected override void InternalHandle(Output result, string args, DependencyGraph graph)
		{
			OutputReferences(result, args, graph, "references");
		}
	}
}
