using System;
using System.Collections.Generic;

namespace org.pescuma.dotnetdependencychecker
{
	public static class LinqHelper
	{
		public static void ForEach<T>(this IEnumerable<T> list, Action<T> cb)
		{
			foreach (var i in list)
				cb(i);
		}
	}
}
