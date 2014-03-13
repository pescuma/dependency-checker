using System.Collections.Generic;

namespace org.pescuma.dependencychecker.presenter.output.results
{
	public interface EntryOutputer
	{
		void Output(List<OutputEntry> entries);
	}
}
