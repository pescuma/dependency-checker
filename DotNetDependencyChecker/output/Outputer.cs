using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.output
{
	public interface Outputer
	{
		void Output(List<OutputEntry> entries);
	}
}
