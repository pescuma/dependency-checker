using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.pescuma.dependencychecker.utils
{
	public class DotStringBuilder
	{
		private const string INDENT = "    ";

		private readonly StringBuilder dot = new StringBuilder();
		private readonly Dictionary<object, int> ids = new Dictionary<object, int>();
		private string indent = "";
		private bool lastWasSpace;

		public DotStringBuilder(string digraphName)
		{
			dot.Append("digraph ")
				.Append(digraphName)
				.Append(" {\n");

			IncreaseIndent();
		}

		private void InternalAppend(string text)
		{
			dot.Append(indent)
				.Append(text)
				.Append("\n");

			lastWasSpace = string.IsNullOrWhiteSpace(text);
		}

		public DotStringBuilder AppendSpace()
		{
			if (lastWasSpace)
				return this;

			InternalAppend("");

			return this;
		}

		public DotStringBuilder AppendLine(string text)
		{
			InternalAppend(text + ";");

			return this;
		}

		public DotStringBuilder AppendConfig(string name, string value)
		{
			AppendLine(name + "=" + FormatValue(value));

			return this;
		}

		public DotStringBuilder AppendNode(object obj, string name, params string[] attibutes)
		{
			dot.Append(indent)
				.Append(Id(obj));

			AppendAttibutes(Concat(attibutes, "label", name));

			dot.Append(";\n");

			lastWasSpace = false;

			return this;
		}

		public DotStringBuilder AppendEdge(object source, object target, params string[] attibutes)
		{
			if (!ids.ContainsKey(source))
				ids.Add(source, ids.Count + 1);

			dot.Append(indent)
				.Append(Id(source))
				.Append(" -> ")
				.Append(Id(target));

			AppendAttibutes(attibutes);

			dot.Append(";\n");

			lastWasSpace = false;

			return this;
		}

		public DotStringBuilder StartSubgraph(string name)
		{
			InternalAppend("subgraph " + name + "{");
			IncreaseIndent();
			return this;
		}

		public DotStringBuilder EndSubgraph()
		{
			DecreaseIndent();
			InternalAppend("}");
			return this;
		}

		public DotStringBuilder StartGroup()
		{
			InternalAppend("{");
			IncreaseIndent();
			return this;
		}

		public DotStringBuilder EndGroup()
		{
			DecreaseIndent();
			InternalAppend("}");
			return this;
		}

		private int Id(object obj)
		{
			if (!ids.ContainsKey(obj))
				ids.Add(obj, ids.Count + 1);
			return ids[obj];
		}

		private string[] Concat(string[] x, params string[] y)
		{
			var z = new string[x.Length + y.Length];
			x.CopyTo(z, 0);
			y.CopyTo(z, x.Length);
			return z;
		}

		private void AppendAttibutes(string[] attibutes)
		{
			var result = new List<string>();

			if (!attibutes.Any())
				return;

			for (int i = 0; i < attibutes.Length; i += 2)
			{
				var name = attibutes[i];
				var value = attibutes[i + 1];

				if (value == null)
					continue;

				result.Add(name + "=" + FormatValue(value));
			}

			if (result.Any())
				dot.Append(" [")
					.Append(string.Join(",", result))
					.Append("]");
		}

		private static string FormatValue(string value)
		{
			value = value.Replace('"', ' ');

			if (value.Length < 1 || value.IndexOfAny(new[] { ' ', '.' }) >= 0)
				value = '"' + value + '"';

			return value;
		}

		public void IncreaseIndent()
		{
			indent += INDENT;
		}

		public void DecreaseIndent()
		{
			indent = indent.Substring(0, indent.Length - INDENT.Length);
		}

		public override string ToString()
		{
			return dot + "}\n";
		}
	}
}
