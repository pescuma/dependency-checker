using System.Collections.Generic;
using org.pescuma.dependencychecker.model;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.output
{
	public interface OutputEntry
	{
		string Type { get; }
		Severity Severity { get; }
		OutputMessage Messsage { get; }
		List<Library> Projects { get; }
		List<Dependency> Dependencies { get; }
	}
}
