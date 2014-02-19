using System;
using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class XMLOutputer : Outputer
	{
		private readonly string file;

		public XMLOutputer(string file)
		{
			this.file = file;
		}

		public void Output(List<OutputEntry> entries)
		{
			throw new NotImplementedException();
		}
	}
}
