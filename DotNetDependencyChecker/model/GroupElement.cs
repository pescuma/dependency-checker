using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class GroupElement : Dependable
	{
		public readonly Group Group;
		public readonly ConfigLocation Location;
		public readonly Dependable Representing;

		public string Name
		{
			get { return Group.Name; }
		}

		public GroupElement(Group group, ConfigLocation location, Dependable representing)
		{
			Group = group;
			Location = location;
			Representing = representing;
		}

		IEnumerable<string> Dependable.Names
		{
			get { return Group.Name.AsList(); }
		}

		IEnumerable<string> Dependable.Paths
		{
			get { return Enumerable.Empty<string>(); }
		}

		public override string ToString()
		{
			return string.Format("{0} representing: {1}", Name, Representing);
		}
	}
}
