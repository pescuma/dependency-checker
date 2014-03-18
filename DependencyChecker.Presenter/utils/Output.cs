using System;
using System.Text;

namespace org.pescuma.dependencychecker.presenter.utils
{
	public class Output
	{
		private string prefix;
		private readonly StringBuilder text = new StringBuilder();
		private string indent = "";

		public Output(string prefix)
		{
			this.prefix = prefix;
		}

		public Output IncreaseIndent()
		{
			indent += prefix;
			return this;
		}

		public Output DecreaseIndent()
		{
			indent = indent.Substring(0, indent.Length - prefix.Length);
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

		public override string ToString()
		{
			return text.ToString();
		}

		public void ToConsole()
		{
			Console.WriteLine(ToString()
				.TrimEnd());
		}

		public class LineOutput
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
