using System.Collections.Generic;
using org.pescuma.dependencychecker.presenter.architecture;

namespace org.pescuma.dependencychecker.presenter.output.architeture
{
	public interface ArchitectureOutputer
	{
		void Output(ArchitectureGraph architecture, List<OutputEntry> warnings);
	}
}
