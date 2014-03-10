using org.pescuma.dependencychecker.config;
using org.pescuma.dependencychecker.rules;

namespace org.pescuma.dependencychecker.output
{
	public class UnusedConfigOutputEntry : BaseOutputEntry
	{
		public readonly ConfigLocation Location;

		public UnusedConfigOutputEntry(string type, ConfigLocation location)
			: base("Config/Unused " + type, Severity.Info, CreateMessage(type, location))
		{
			Location = location;
		}

		private static OutputMessage CreateMessage(string type, ConfigLocation location)
		{
			return new OutputMessage().Append("The ")
				.Append(type)
				.Append(" defined in ")
				.Append(location.LineNum)
				.Append(" of config file is never used: ")
				.Append(location.LineText);
		}
	}
}
