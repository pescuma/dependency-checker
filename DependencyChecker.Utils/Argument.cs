﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace org.pescuma.dependencychecker.utils
{
	[DebuggerStepThrough]
	public class Argument
	{
		// TODO User field names in ArgumentException

		public static void ThrowIfNull<T>(T param) where T : class
		{
			if (param == null)
				throw new ArgumentNullException();
		}

		public static void ThrowIfNull<T>(T? param) where T : struct
		{
			if (param == null)
				throw new ArgumentNullException();
		}

		public static void ThrowIfAllNull(params object[] fields)
		{
			if (fields.All(f => f == null))
				throw new ArgumentNullException();
		}

		internal static void ThrowIfEmpty<T>(IEnumerable<T> param)
		{
			if (param == null)
				throw new ArgumentNullException();
			if (!param.GetEnumerator()
				.MoveNext())
				throw new ArgumentOutOfRangeException("Deve ter ao menos um item", (Exception) null);
		}

		public static void ThrowIfNullOrWhiteSpace(string param)
		{
			if (param == null)
				throw new ArgumentNullException();
			if (string.IsNullOrWhiteSpace(param))
				throw new ArgumentOutOfRangeException("Não pode ser vazio", (Exception) null);
		}

		public static void ThrowIfFalse(bool param)
		{
			if (!param)
				throw new ArgumentException();
		}

		public static void ThrowIfFalse(bool param, string message)
		{
			if (!param)
				throw new ArgumentException(message);
		}

		public static void ThrowIfOutOfRange(int param, int min, int max)
		{
			if (param < min || param >= max)
				throw new ArgumentOutOfRangeException("Deveria ser na faixa [" + min + ", " + max + "(");
		}
	}
}
