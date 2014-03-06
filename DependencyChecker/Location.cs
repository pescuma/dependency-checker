namespace org.pescuma.dependencychecker
{
	public class Location
	{
		public readonly string File;
		public readonly int Line;

		public Location(string file, int line)
		{
			File = file;
			Line = line;
		}
	}
}
