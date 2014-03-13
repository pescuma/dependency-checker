using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencyconsole.commands
{
	internal class ReferencedByCommand : BaseReferencesCommand
	{
		public override string Name
		{
			get { return "referenced by"; }
		}

		protected override void InternalHandle(string args, DependencyGraph graph)
		{
			OutputReferences(args, graph, "references");
		}
	}
}
