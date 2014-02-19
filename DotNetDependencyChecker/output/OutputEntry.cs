using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public interface OutputEntry
	{
		Severity Severity { get; }
		OutputMessage Messsage { get; }
		List<Dependable> Projects { get; }
		List<Dependency> Dependencies { get; }
	}
}
