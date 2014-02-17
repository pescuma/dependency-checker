using System.Collections.Generic;
using System.Linq;
using org.pescuma.dotnetdependencychecker.model;

namespace org.pescuma.dotnetdependencychecker.rules
{
	public class OutputMessage
	{
		private List<Element> elements = new List<Element>();

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

		public string ToConsole()
		{
			return string.Join("", elements.Select(e =>
			{
				if (e.Text != null)
				{
					return e.Text;
				}
				else if (e.Project is Project)
				{
					var proj = ((Project) e.Project);
					if (e.Info == Info.NameAndPath)
						return proj.GetNameAndPath();
					else
						return proj.GetCsprojOrFullID();
				}
				else
					return e.Project.Names.First();
			}));
		}

		public enum Info
		{
			NameAndPath,
			CsprojOrFullID
		}

		private class Element
		{
			internal readonly string Text;
			internal readonly Dependable Project;
			internal readonly Info Info;

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
