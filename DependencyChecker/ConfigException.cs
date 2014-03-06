using System;

namespace org.pescuma.dependencychecker
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
