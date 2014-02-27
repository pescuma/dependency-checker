using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker.utils
{
	internal static class ExtensionMethods
	{
		public static TV Get<TK, TV>(this Dictionary<TK, TV> dict, TK key) where TV : class
		{
			TV result;
			if (dict.TryGetValue(key, out result))
				return result;
			else
				return null;
		}
	}
}
