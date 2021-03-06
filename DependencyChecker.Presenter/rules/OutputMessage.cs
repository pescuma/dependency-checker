﻿using System.Collections.Generic;
using org.pescuma.dependencychecker.model;

namespace org.pescuma.dependencychecker.presenter.rules
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

		public OutputMessage Append(Library proj, ProjInfo info)
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
			NameAndPath,
			NameAndProjectPath,
			NameAndGroup,
			Path,
			ProjectPath,
		}

		public enum DepInfo
		{
			Type,
			Line,
			FullDescription
		}

		public class Element
		{
			public readonly string Text;
			public readonly Library Project;
			public readonly ProjInfo ProjInfo;
			public readonly Dependency Dependendcy;
			public readonly DepInfo DepInfo;

			public Element(string text)
			{
				Text = text ?? "<null>";
				Project = null;
			}

			public Element(Library project, ProjInfo projInfo)
			{
				Project = project;
				ProjInfo = projInfo;
				Text = null;
			}

			public Element(Dependency dependendcy, DepInfo depInfo)
			{
				Dependendcy = dependendcy;
				DepInfo = depInfo;
			}
		}
	}
}
