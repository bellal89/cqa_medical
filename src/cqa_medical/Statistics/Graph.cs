using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Statistics
{
	public struct Vertex
	{
		public int Id { get; set; }
		public double Weight { get; set; }
		public string Label { get; set; }
	}

	public struct Edge
	{
		public int SourceId { get; set; }
		public int DestinationId { get; set; }
		public double Weight { get; set; }
	}

	public class GraphBuilder
	{
		private readonly List<Vertex> vertices;
		private readonly List<Edge> edges;

		public GraphBuilder(IEnumerable<Edge> graphEdges)
		{
			edges = graphEdges.ToList();
			var vertexIds = new HashSet<int>();
			foreach (var edge in edges)
			{
				vertexIds.Add(edge.SourceId);
				vertexIds.Add(edge.DestinationId);
			}
			vertices = vertexIds.Select(v => new Vertex { Id = v, Label = v.ToString(CultureInfo.InvariantCulture), Weight = 1 }).ToList();
		}

		public GraphBuilder(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
		{
			this.vertices = vertices.ToList();
			this.edges = edges.ToList();
		}

		public GraphBuilder(IEnumerable<int> vertices, IEnumerable<Tuple<int, int>> edges )
		{
			this.vertices = vertices.Select(v => new Vertex {Id = v, Label = v.ToString(CultureInfo.InvariantCulture), Weight = 1}).ToList();
			this.edges = edges.Select(e => new Edge {SourceId = e.Item1, DestinationId = e.Item2, Weight = 1}).ToList();
		}

		public GraphBuilder(IEnumerable<string> vertexLabels, IEnumerable<Tuple<string, string>> edges)
		{
			var labelToVertex = vertexLabels.Select((label, id) => new Vertex {Id = id, Label = label, Weight = 1}).ToDictionary(v => v.Label, v => v);
			vertices = labelToVertex.Values.ToList();

			this.edges =
				edges.Select(e => new Edge {SourceId = labelToVertex[e.Item1].Id, DestinationId = labelToVertex[e.Item2].Id, Weight = 1}).ToList();
		}

		public void ExportToGVFormat(string fileToSave, string graphName, bool isOriented)
		{
			var relationMark = isOriented ? " -> " : " -- ";
			var graphMark = isOriented ? "digraph" : "graph";

			var verticesString = String.Join("\n", vertices.Select(v => String.Format("\t{0} [label=\"{1}\" weight={2}];", v.Id, v.Label, v.Weight)));
			var edgesString = String.Join("\n", edges.Select(e => String.Format("\t{0} {1} {2} [weight={3}];", e.SourceId, relationMark, e.DestinationId, e.Weight)));

			File.WriteAllText(fileToSave, String.Format("{0} {1}\n{{\n{2}\n{3}\n}}", graphMark, graphName, verticesString, edgesString));
		}

		public void ExportToPajecFormat(string fileToSave)
		{
			throw new NotImplementedException();
		}
	}

	class Graph
	{
		public Dictionary<string, Dictionary<string,int>> AdjacencyList;

		public Graph(QuestionList ql)
		{
			AdjacencyList = new Dictionary<string, Dictionary<string, int>>();
			foreach (var question in ql.GetAllQuestions())
			{
				foreach (var answer in question.GetAnswers())
				{
					var from = answer.AuthorEmail;
					var to = question.AuthorEmail;
					if (!AdjacencyList.ContainsKey(from))
						AdjacencyList.Add(from, new Dictionary<string, int>());
					AdjacencyList[from].UpdateOrAdd(to, v => v + 1, 1);
				}
			}
		}
	}

	internal class VertexCoder
	{
		public Dictionary<string, int> NameToNumber = new Dictionary<string, int>();
		public VertexCoder(IEnumerable<string> names )
		{
			int i = 1;
			foreach (var name in names)
			{
				NameToNumber.Add(name, i++);
			}
		}
	}




	[TestFixture]
	internal class GraphProgram
	{
		private IEnumerable<Tuple<string,string,int>> GetEdges(Dictionary<string, Dictionary<string,int>> adjList)
		{
			foreach (var pair in adjList)
				foreach (var v in pair.Value)
					yield return new Tuple<string, string, int >(pair.Key, v.Key, v.Value);
		}

		[Test]
		public void GenerateGraph()
		{
			var g = new Graph(Program.DefaultQuestionList.NewQuestionListFilteredByTopics(26));
			var adjacencyList = g.AdjacencyList;
			var allVertexes = adjacencyList.SelectMany(k => new[] {k.Key}.Concat(k.Value.Keys)).Distinct().ToList();
//			adjacencyList
//				.Where(k => k.Value.Count > 10)
//				.OrderByDescending(k => k.Value.Count)
//				.Select(k => k.FormatString())
			var nameToNumber = new VertexCoder(allVertexes).NameToNumber;
			File.WriteAllText("ggraphFlu.net",
			                  string.Format("*Vertices      {0}\n{1}\n*Arcs\n{2}",
			                                allVertexes.Count,
			                                nameToNumber.ToStringInverted("   "),
			                                string.Join("\n",
			                                            GetEdges(adjacencyList).Select(
															k => nameToNumber[k.Item1] + "   " + nameToNumber[k.Item2] + "  " + k.Item3))

			                  	));
		}
	}

//	internal class NodeNameGenerator
//	{
//		private Dictionary<string, int> nodeName;
//		private Dictionary<string, int> node;
//		private int i;
//
//		public NodeNameGenerator()
//		{
//			nodeName = new Dictionary<string, int>();
//			i = 0;
//		}
//		public string GetName(int codeNumber)
//		{
//			if (nodeName.ContainsKey())
//			return 
//		}
//	}
}
