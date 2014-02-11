using System;

namespace org.pescuma.dotnetdependencychecker.config
{
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
	}
}
