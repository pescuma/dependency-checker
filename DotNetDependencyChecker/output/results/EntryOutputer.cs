using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.output.results
{
	public interface EntryOutputer
	{
		void Output(List<OutputEntry> entries);
	}
}
