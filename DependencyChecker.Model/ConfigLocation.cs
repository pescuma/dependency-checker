namespace org.pescuma.dependencychecker.model
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

		protected bool Equals(ConfigLocation other)
		{
			return LineNum == other.LineNum && string.Equals(LineText, other.LineText);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((ConfigLocation) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (LineNum * 397) ^ (LineText != null ? LineText.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", LineNum, LineText);
		}
	}
}
