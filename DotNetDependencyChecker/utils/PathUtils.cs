using System.IO;

namespace org.pescuma.dotnetdependencychecker.utils
{
	internal class PathUtils
	{
		public static string ToAbsolute(string root, string path)
		{
			if (Path.IsPathRooted(path))
				return Path.GetFullPath(path);
			else
				return Path.GetFullPath(Path.Combine(root, path));
		}
	}
}
