using QuickGraph;

namespace org.pescuma.dependencychecker
{
	public class Dependency : Edge<Library>
	{
		public readonly string Type;
		public readonly string ReferencedPath;

		public Dependency(Library source, Library target, string type, string referencedPath)
			: base(source, target)
		{
			Type = type;
			ReferencedPath = referencedPath;
		}
	}
}
