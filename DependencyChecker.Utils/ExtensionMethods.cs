using System;
using System.Collections.Generic;
using System.Linq;

namespace org.pescuma.dependencychecker.utils
{
	public static class ExtensionMethods
	{
		public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, TK key) where TV : class
		{
			TV result;
			if (dict.TryGetValue(key, out result))
				return result;
			else
				return null;
		}

		public static void AddRange<T>(this ISet<T> set, IEnumerable<T> toAdd)
		{
			foreach (var e in toAdd)
				set.Add(e);
		}

		public static string EmptyIfNull(this string obj)
		{
			return obj ?? "";
		}

		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> obj)
		{
			return obj ?? Enumerable.Empty<T>();
		}

		public static List<T> EmptyIfNull<T>(this List<T> obj)
		{
			return obj ?? new List<T>();
		}

		public static Func<T, bool> And<T>(this Func<T, bool> p1, Func<T, bool> p2)
		{
			if (p1 == null)
				return p2;

			if (p2 == null)
				return p1;

			return t => p1(t) && p2(t);
		}

		public static Func<T, bool> Or<T>(this Func<T, bool> p1, Func<T, bool> p2)
		{
			if (p1 == null)
				return p2;

			if (p2 == null)
				return p1;

			return t => p1(t) || p2(t);
		}

		public static Func<T, bool> Not<T>(this Func<T, bool> p1)
		{
			return t => !p1(t);
		}
	}
}
