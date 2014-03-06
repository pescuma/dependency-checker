using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class GroupElement : Library
	{
		public readonly Group Group;
		public readonly ConfigLocation Location;
		public readonly Library Representing;

		public GroupElement(Group group, ConfigLocation location, Library representing)
			: base(group.Name)
		{
			Group = group;
			Location = location;
			Representing = representing;
		}

		public override string ToString()
		{
			return string.Format("{0} representing: {1}", Name, Representing);
		}
	}
}
