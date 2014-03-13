using System.Collections.Generic;

namespace org.pescuma.dependencychecker.output.results
{
	public interface EntryOutputer
	{
		void Output(List<OutputEntry> entries);
	}
}
