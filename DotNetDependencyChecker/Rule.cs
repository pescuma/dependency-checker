using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.config;
using QuickGraph.Algorithms;

namespace org.pescuma.dotnetdependencychecker
{
	public class RuleMatch
	{
		public readonly bool Allowed;
		public readonly string Messsage;
		public readonly ConfigLocation Location;
		public readonly List<Dependency> Dependencies;

		public RuleMatch(bool allowed, string messsage, ConfigLocation location, List<Dependency> dependencies)
		{
			Allowed = allowed;
			Messsage = messsage;
			Location = location;
			Dependencies = dependencies;
		}

		public RuleMatch(bool allowed, string messsage, ConfigLocation location, params Dependency[] dependencies)
			: this(allowed, messsage, location, dependencies.ToList())
		{
		}
	}

	public interface Rule
	{
		List<RuleMatch> Process(DependencyGraph graph);

		/// <returns>null if didn't match</returns>
		RuleMatch Process(Dependency dep);
	}

	public class DepenendencyRule : Rule
	{
		// HACK [pescuma] Fields are public for tests
		public readonly Func<Project, bool> Source;
		public readonly Func<Project, bool> Target;
		public readonly bool Allow;
		private readonly ConfigLocation location;

		public DepenendencyRule(Func<Project, bool> source, Func<Project, bool> target, bool allow, ConfigLocation location)
		{
			Source = source;
			Target = target;
			Allow = allow;
			this.location = location;
		}

		public List<RuleMatch> Process(DependencyGraph graph)
		{
			return null;
		}

		public RuleMatch Process(Dependency dep)
		{
			if (!Source(dep.Source) || !Target(dep.Target))
				return null;

			return new RuleMatch(Allow,
				string.Format("Dependence between {0} and {1} {2}allowed", dep.Source.ToGui(), dep.Target.ToGui(), Allow ? "" : "not "), location, dep);
		}
	}

	public class NoCircularDepenendenciesRule : Rule
	{
		private readonly ConfigLocation location;

		public NoCircularDepenendenciesRule(ConfigLocation location)
		{
			this.location = location;
		}

		public List<RuleMatch> Process(DependencyGraph graph)
		{
			var result = new List<RuleMatch>();

			IDictionary<Project, int> components;
			graph.StronglyConnectedComponents(out components);

			var circularDependencies = components.Select(c => new { Proj = c.Key, Group = c.Value })
				.GroupBy(c => c.Group)
				.Where(g => g.Count() > 1);

			foreach (var g in circularDependencies)
			{
				var projs = g.Select(i => i.Proj)
					.ToList();
				projs.Sort(Project.NaturalOrdering);

				var projsSet = new HashSet<Project>(projs);
				var deps = graph.Edges.Where(e => projsSet.Contains(e.Source) && projsSet.Contains(e.Target))
					.ToList();
				deps.Sort(Dependency.NaturalOrdering);

				var err = new StringBuilder();
				err.Append("Circular dependency found:");
				projs.ForEach(p => err.Append("\n  - ")
					.Append(p.Name));

				result.Add(new RuleMatch(false, err.ToString(), location, deps));
			}

			return result;
		}

		public RuleMatch Process(Dependency dep)
		{
			return null;
		}
	}
}
