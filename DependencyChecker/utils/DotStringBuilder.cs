using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.pescuma.dependencychecker.utils
{
	public class DotStringBuilder
	{
		private const string INDENT = "    ";

		private readonly StringBuilder dot = new StringBuilder();
		private readonly Dictionary<string, int> ids = new Dictionary<string, int>();
		private string indent = "";

		public DotStringBuilder(string digraphName)
		{
			dot.Append("digraph ")
				.Append(digraphName)
				.Append(" {\n");

			IncreaseIndent();
		}

		public void AppendSpace()
		{
			dot.Append(indent)
				.Append("\n");
		}

		public DotStringBuilder AppendNode(string name, params string[] attibutes)
		{
			dot.Append(indent)
				.Append(Id(name));

			AppendAttibutes(Concat(attibutes, "label", name));

			dot.Append(";\n");

			return this;
		}

		public DotStringBuilder AppendEdge(string source, string target, params string[] attibutes)
		{
			if (!ids.ContainsKey(source))
				ids.Add(source, ids.Count + 1);

			dot.Append(indent)
				.Append(Id(source))
				.Append(" -> ")
				.Append(Id(target));

			AppendAttibutes(attibutes);

			dot.Append(";\n");

			return this;
		}

		private int Id(string name)
		{
			if (!ids.ContainsKey(name))
				ids.Add(name, ids.Count + 1);
			return ids[name];
		}

		private string[] Concat(string[] attibutes, params string[] more)
		{
			return attibutes.Concat(more)
				.ToArray();
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

				value = value.Replace('"', ' ');
				if (value.IndexOf(' ') >= 0)
					value = '"' + value + '"';

				result.Add(name + "=" + value);
			}

			if (result.Any())
				dot.Append(" [")
					.Append(string.Join(",", result))
					.Append("]");
		}

		public void IncreaseIndent()
		{
			indent += INDENT;
		}

		public void DecreaseIndent()
		{
			indent += indent.Substring(0, indent.Length - INDENT.Length);
		}

		public override string ToString()
		{
			return dot + "}\n";
		}
	}
}
