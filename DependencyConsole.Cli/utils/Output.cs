using System;
using System.Text;

namespace org.pescuma.dependencyconsole.utils
{
	internal class Output
	{
		private const string PREFIX = "    ";

		private readonly StringBuilder text = new StringBuilder();
		private string indent = "";

		public Output IncreaseIndent()
		{
			indent += PREFIX;
			return this;
		}

		public Output DecreaseIndent()
		{
			indent = indent.Substring(0, indent.Length - PREFIX.Length);
			return this;
		}

		public Output AppendLine(StringBuilder result)
		{
			AppendLine(result.ToString());
			return this;
		}

		public Output AppendLine(string value, params object[] args)
		{
			text.Append(indent);
			if (args.Length < 1)
				text.Append(value);
			else
				text.AppendFormat(value, args);
			text.AppendLine();
			return this;
		}

		public Output AppendLine(int value)
		{
			text.Append(indent)
				.Append(value)
				.AppendLine();
			return this;
		}

		public Output AppendLine(double value)
		{
			text.Append(indent)
				.Append(value)
				.AppendLine();
			return this;
		}

		public Output AppendLine()
		{
			text.Append(indent)
				.AppendLine();
			return this;
		}

		public LineOutput StartLine()
		{
			text.Append(indent);
			return new LineOutput(text);
		}

		public void ToConsole()
		{
			Console.WriteLine(text.ToString()
				.TrimEnd());
		}

		internal class LineOutput
		{
			private readonly StringBuilder text;

			internal LineOutput(StringBuilder text)
			{
				this.text = text;
			}

			public LineOutput Append(string value, params object[] args)
			{
				if (args.Length < 1)
					text.Append(value);
				else
					text.AppendFormat(value, args);
				return this;
			}

			public LineOutput Append(int value)
			{
				text.Append(value);
				return this;
			}

			public LineOutput Append(double value)
			{
				text.Append(value);
				return this;
			}

			public void EndLine()
			{
				text.AppendLine();
			}
		}
	}
}
