using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker.output.dependencies
{
	public class DotDependenciesOutputer : DependenciesOutputer
	{
		private const string INDENT = "    ";

		private readonly string file;
		private readonly Dictionary<Dependable, int> ids = new Dictionary<Dependable, int>();

		private DependencyGraph graph;
		private List<OutputEntry> warnings;

		public DotDependenciesOutputer(string file)
		{
			this.file = file;
		}

		public void Output(DependencyGraph aGraph, List<OutputEntry> aWarnings)
		{
			//graph = Sample(aGraph, aWarnings);
			graph = aGraph;
			warnings = aWarnings;

			graph.Vertices.ForEach((proj, i) => ids.Add(proj, i + 1));

			var result = GenerateDot();

			File.WriteAllText(file, result);
		}

		private static DependencyGraph Sample(DependencyGraph aGraph, List<OutputEntry> aWarnings)
		{
			var rand = new Random();
			var selected = new HashSet<Dependable>(aGraph.Vertices.Where(v => rand.Next(10) < 1 || aWarnings.Any(w => w.Projects.Contains(v))));

			var graph = new DependencyGraph();
			graph.AddVertexRange(selected);
			graph.AddEdgeRange(aGraph.Edges.Where(e => selected.Contains(e.Source) && selected.Contains(e.Target)));

			return graph;
		}

		private string GenerateDot()
		{
			var result = new StringBuilder();

			result.Append("digraph Dependencies {\n");
			result.Append(INDENT)
				.Append("concentrate=true;\n");

			int clusterIndex = 1;

			foreach (var group in graph.Vertices.OfType<Assembly>()
				.Where(a => a.GroupElement != null)
				.GroupBy(a => a.GroupElement.Name))
			{
				result.Append(INDENT)
					.Append("subgraph cluster")
					.Append(clusterIndex++)
					.Append("{\n");

				const string subprefix = INDENT + INDENT;

				var projs = new HashSet<Dependable>(@group);

				projs.ForEach(a => AppendProject(result, subprefix, a));

				result.Append("\n");

				graph.Edges.Where(e => projs.Contains(e.Source) && projs.Contains(e.Target))
					.ForEach(d => AppendDependency(result, subprefix, d));

				result.Append("\n");

				result.Append(subprefix)
					.Append("label=\"")
					.Append(@group.Key.Replace('"', ' '))
					.Append("\";\n");
				result.Append(subprefix)
					.Append("color=lightgray;\n");
				result.Append(subprefix)
					.Append("style=filled;\n");
				result.Append(subprefix)
					.Append("fontsize=20;\n");

				result.Append(INDENT)
					.Append("}\n\n");
			}

			graph.Vertices.OfType<Assembly>()
				.Where(a => a.GroupElement == null)
				.ForEach(a => AppendProject(result, INDENT, a));

			result.Append("\n");

			graph.Edges.Where(e => AreFromDifferentGroups(e.Source, e.Target))
				.ForEach(d => AppendDependency(result, INDENT, d));

			result.Append("}\n");

			return result.ToString();
		}

		private bool AreFromDifferentGroups(Dependable source, Dependable target)
		{
			var p1 = (Assembly) source;
			var p2 = (Assembly) target;

			if (p1.GroupElement == null || p2.GroupElement == null)
				return true;

			return p1.GroupElement.Name != p2.GroupElement.Name;
		}

		private void AppendProject(StringBuilder result, string prefix, Dependable assembly)
		{
			var id = ids[assembly];
			result.Append(prefix)
				.Append(id)
				.Append(" [");

			if (assembly is Project)
				result.Append("shape=box");
			else
				result.Append("shape=ellipse");

			result.Append(",label=\"")
				.Append(assembly.Names.First()
					.Replace('"', ' '))
				.Append("\"");

			var color = GetColor(warnings.Where(w => w.Projects.Contains(assembly)));
			if (color != null)
				result.Append(",color=\"")
					.Append(color)
					.Append("\"");

			result.Append("];\n");
		}

		private void AppendDependency(StringBuilder result, string prefix, Dependency dep)
		{
			result.Append(prefix)
				.Append(ids[dep.Source])
				.Append(" -> ")
				.Append(ids[dep.Target]);

			var attribs = new List<string>();

			if (dep.Type == Dependency.Types.DllReference)
				attribs.Add("style=dashed");

			var color = GetColor(warnings.Where(w => w.Dependencies.Contains(dep)));
			if (color != null)
				attribs.Add("color=\"" + color + "\"");

			if (attribs.Any())
				result.Append(" [")
					.Append(string.Join(",", attribs))
					.Append("]");

			result.Append(";\n");
		}

		private string GetColor(IEnumerable<OutputEntry> warns)
		{
			var serverities = warns.Select(p => p.Severity)
				.Distinct()
				.ToList();

			if (serverities.Any(s => s == Severity.Error))
				return "red";
			else if (serverities.Any(s => s == Severity.Warning))
				return "darkgoldenrod2";
			else if (serverities.Any(s => s == Severity.Info))
				return "blue";
			else
				return null;
		}
	}
}
