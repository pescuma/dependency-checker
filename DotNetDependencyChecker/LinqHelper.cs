﻿using System;
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

		public static IEnumerable<IndexedElement<T>> Indexed<T>(this IEnumerable<T> list)
		{
			int index = -1;
			foreach (var i in list)
				yield return new IndexedElement<T>(i, ++index);
		}

		public class IndexedElement<T>
		{
			public readonly T Item;
			public readonly int Index;

			public IndexedElement(T item, int index)
			{
				Item = item;
				Index = index;
			}

			public IndexedElement<T> WithItem(T other)
			{
				return new IndexedElement<T>(other, Index);
			}
		}
	}
}
