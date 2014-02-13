namespace org.pescuma.dotnetdependencychecker.config
{
	public class ConfigLocation
	{
		public readonly int LineNum;
		public readonly string LineText;

		public ConfigLocation(int lineNum, string lineText)
		{
			LineNum = lineNum;
			LineText = lineText;
		}
	}
}
