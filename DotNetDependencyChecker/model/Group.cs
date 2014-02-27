﻿namespace org.pescuma.dotnetdependencychecker.model
{
	public class Group
	{
		public readonly string Name;

		public Group(string name)
		{
			Name = name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
