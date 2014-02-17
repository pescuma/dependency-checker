using System.Collections.Generic;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class OutputMessage
	{
		private List<Element> elements = new List<Element>();

		public IEnumerable<Element> Elements
		{
			get { return elements; }
		}

		public OutputMessage Append(string text)
		{
			elements.Add(new Element(text));
			return this;
		}

		public OutputMessage Append(int text)
		{
			elements.Add(new Element(text.ToString()));
			return this;
		}

		public OutputMessage Append(Dependable proj, Info info)
		{
			elements.Add(new Element(proj, info));
			return this;
		}

		public enum Info
		{
			Name,
			Csproj
		}

		public class Element
		{
			public readonly string Text;
			public readonly Dependable Project;
			public readonly Info Info;

			public Element(string text)
			{
				Text = text;
				Project = null;
			}

			public Element(Dependable project, Info info)
			{
				Project = project;
				this.Info = info;
				Text = null;
			}
		}
	}
}
