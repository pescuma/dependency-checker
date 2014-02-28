using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace org.pescuma.dotnetdependencychecker.output.results
{
	public class TextEntryOutputer : EntryOutputer
	{
		private readonly string file;

		public TextEntryOutputer(string file)
		{
			this.file = file;
		}

		public void Output(List<OutputEntry> entries)
		{
			var text = string.Join("\n\n", entries.Select(e => ConsoleEntryOutputer.ToConsole(e, true))) + "\n";

			var dir = Path.GetDirectoryName(file);
			if (dir != null)
				Directory.CreateDirectory(dir);

			File.WriteAllText(file, text);
		}
	}
}
