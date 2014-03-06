using System;

namespace org.pescuma.dependencychecker.config
{
	[Serializable]
	public class ConfigParserException : Exception
	{
		public ConfigParserException(string message)
			: base(message)
		{
		}

		public ConfigParserException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public ConfigParserException(ConfigLocation location, string message)
			: this(string.Format("{0} in line {1}: {2}", message, location.LineNum, location.LineText))
		{
		}
	}
}
