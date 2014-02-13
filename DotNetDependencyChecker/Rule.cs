using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph.Algorithms;

namespace org.pescuma.dotnetdependencychecker
{
	public interface Rule
	{
		void Start(List<string> result, DependencyGraph graph);
		bool Process(List<string> result, Dependency dep);
		void Finish(List<string> result, DependencyGraph graph);
	}

	// Fields are public for tests
	public class DepenendencyRule : Rule
	{
		public readonly Func<Project, bool> Source;
		public readonly Func<Project, bool> Target;
		public readonly bool Allow;
		private readonly ConfigLocation location;

		public DepenendencyRule(Func<Project, bool> source, Func<Project, bool> target, bool allow, ConfigLocation location)
		{
			this.Source = source;
			this.Target = target;
			this.Allow = allow;
			this.location = location;
		}

		public void Start(List<string> result, DependencyGraph graph)
		{
		}

		public bool Process(List<string> result, Dependency dep)
		{
			if (!Source(dep.Source) || !Target(dep.Target))
				return false;

			if (!Allow)
				result.Add(string.Format("Dependence between {0} and {1} not allowed", dep.Source.ToGui(), dep.Target.ToGui()));

			return true;
		}

		public void Finish(List<string> result, DependencyGraph graph)
		{
		}
	}

	public class NoCircularDepenendenciesRule : Rule
	{
		private readonly ConfigLocation location;

		public NoCircularDepenendenciesRule(ConfigLocation location)
		{
			this.location = location;
		}

		public void Start(List<string> result, DependencyGraph graph)
		{
			IDictionary<Project, int> components;
			graph.StronglyConnectedComponents(out components);

			var circularDependencies = components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1);

			foreach (var g in circularDependencies)
			{
				var err = new StringBuilder();
				err.Append("Circular dependency found:");
				g.ForEach(p => err.Append("\n  - ")
					.Append(p.Proj.Name));

				result.Add(err.ToString());
			}
		}

		public bool Process(List<string> result, Dependency dep)
		{
			return false;
		}

		public void Finish(List<string> result, DependencyGraph graph)
		{
		}
	}

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
