using System;
using System.IO;

namespace org.pescuma.dependencychecker.utils
{
	public class PathUtils
	{
		public static string ToAbsolute(string root, string path)
		{
			if (Path.IsPathRooted(path))
				return Path.GetFullPath(path);
			else
				return Path.GetFullPath(Path.Combine(root, path));
		}

		public static bool PathMatches(string fullPath, string beginPath)
		{
			if (fullPath == null || beginPath == null)
				return false;

			return fullPath.Equals(beginPath, StringComparison.CurrentCultureIgnoreCase)
			       || fullPath.StartsWith(beginPath + "\\", StringComparison.CurrentCultureIgnoreCase);
		}

		public static string RemoveSeparatorAtEnd(string absolute)
		{
			var last = absolute.Length-1;

			if (last < 0)
				return absolute;

			else if (absolute[last] == Path.DirectorySeparatorChar)
				return absolute.Substring(0, last);

			else
				return absolute;
		}
	}
}
