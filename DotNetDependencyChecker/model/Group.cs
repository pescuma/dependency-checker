using System.Collections.Generic;
using System.Linq;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class Group : Dependable
	{
		public readonly string Name;

		public Group(string name)
		{
			Name = name;
		}

		IEnumerable<string> Dependable.Names
		{
			get { return Name.AsList(); }
		}

		IEnumerable<string> Dependable.Paths
		{
			get { return Enumerable.Empty<string>(); }
		}
	}
}
