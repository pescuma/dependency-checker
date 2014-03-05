using System;

namespace org.pescuma.dotnetdependencychecker
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
