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

		public OutputMessage Append(Dependable proj, ProjInfo info)
		{
			elements.Add(new Element(proj, info));
			return this;
		}

		public OutputMessage Append(Dependency dep, DepInfo info)
		{
			elements.Add(new Element(dep, info));
			return this;
		}

		public enum ProjInfo
		{
			Name,
			Path
		}

		public enum DepInfo
		{
			Type,
			Line
		}

		public class Element
		{
			public readonly string Text;
			public readonly Dependable Project;
			public readonly ProjInfo ProjInfo;
			public readonly Dependency Dependendcy;
			public readonly DepInfo DepInfo;

			public Element(string text)
			{
				Text = text;
				Project = null;
			}

			public Element(Dependable project, ProjInfo projInfo)
			{
				Project = project;
				this.ProjInfo = projInfo;
				Text = null;
			}

			public Element(Dependency dependendcy, DepInfo depInfo)
			{
				this.Dependendcy = dependendcy;
				this.DepInfo = depInfo;
			}
		}
	}
}
