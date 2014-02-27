using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.output.errors
{
	public interface EntryOutputer
	{
		void Output(List<OutputEntry> entries);
	}
}
