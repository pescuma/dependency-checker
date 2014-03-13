using System.Collections.Generic;
using org.pescuma.dependencychecker.architecture;

namespace org.pescuma.dependencychecker.output.architeture
{
	public interface ArchitectureOutputer
	{
		void Output(ArchitectureGraph architecture, List<OutputEntry> warnings);
	}
}
