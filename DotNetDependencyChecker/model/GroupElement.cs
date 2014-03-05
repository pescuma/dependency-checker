using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class GroupElement : Assembly
	{
		public readonly Group Group;
		public readonly ConfigLocation Location;
		public readonly Assembly Representing;

		public GroupElement(Group group, ConfigLocation location, Assembly representing)
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
