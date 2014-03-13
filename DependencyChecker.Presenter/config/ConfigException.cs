using System;

namespace org.pescuma.dependencychecker.presenter.config
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
