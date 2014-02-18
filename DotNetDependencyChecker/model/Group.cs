using System.Collections.Generic;
using System.Linq;

namespace org.pescuma.dotnetdependencychecker.model
{
	public class Group : Dependable
	{
		public readonly string Name;
		public readonly Dependable Representing;

		public Group(string name)
		{
			Name = name;
			Representing = null;
		}

		private Group(string name, Dependable representing)
		{
			Name = name;
			Representing = representing;
		}

		IEnumerable<string> Dependable.Names
		{
			get { return Name.AsList(); }
		}

		IEnumerable<string> Dependable.Paths
		{
			get { return Enumerable.Empty<string>(); }
		}

		public Group ActingAs(Dependable proj)
		{
			return new Group(Name, proj);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
