using System;

namespace org.pescuma.dependencychecker.config
{
	[Serializable]
	public class ConfigException : Exception
	{
		public ConfigException(string message)
			: base(message)
		{
		}
	}
}
