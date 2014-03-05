using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output
{
	public interface OutputEntry
	{
		string Type { get; }
		Severity Severity { get; }
		OutputMessage Messsage { get; }
		List<Assembly> Projects { get; }
		List<Dependency> Dependencies { get; }
	}
}
