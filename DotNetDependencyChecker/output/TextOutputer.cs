using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace org.pescuma.dotnetdependencychecker.output
{
	public class TextOutputer : Outputer
	{
		private readonly string file;

		public TextOutputer(string file)
		{
			this.file = file;
		}

		public void Output(List<OutputEntry> entries)
		{
			var text = string.Join("\n\n", entries.Select(e => ConsoleOutputer.ToConsole(e, true))) + "\n";

			var dir = Path.GetDirectoryName(file);
			if (dir != null)
				Directory.CreateDirectory(dir);

			File.WriteAllText(file, text);
		}
	}
}
