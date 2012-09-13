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
			: this(vertexLabels, edges.Select(q => Tuple.Create(q.Item1, q.Item2, 1d)))
		{
			
		}
		public GraphBuilder(IEnumerable<string> vertexLabels, IEnumerable<Tuple<string, string, double>> edges)
		{
			var labelToVertex = vertexLabels.Select((label, id) => new Vertex {Id = id, Label = label, Weight = 1}).ToDictionary(v => v.Label, v => v);
			vertices = labelToVertex.Values.ToList();

			this.edges =
				edges.Select(e => new Edge {SourceId = labelToVertex[e.Item1].Id, DestinationId = labelToVertex[e.Item2].Id, Weight = e.Item3}).ToList();
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

			File.WriteAllText(fileToSave,
			                  string.Format("*Vertices      {0}\n{1}\n*Arcs\n{2}",
			                                vertices.Count,
			                                String.Join("\n", vertices.Select(q => q.Id + " " + q.Label)),
			                                String.Join("\n", edges.Select(k => k.SourceId + " " + k.DestinationId + " " +  k.Weight))));
		}
	}

	class UserGraph  : GraphBuilder
	{

		public UserGraph(QuestionList ql)
			: base(GetAllVertices(ql), GetAllEdges(ql))
		{
		}

		private static IEnumerable<string> GetAllVertices(QuestionList ql)
		{
			return ql.GetAllQuestions().Select(q => q.AuthorEmail).Concat(ql.GetAllAnswers().Select(q => q.AuthorEmail)).Distinct();
		}

		private static IEnumerable<Tuple<string, string, double>> GetAllEdges(QuestionList ql)
		{
			var edges = new Dictionary<Tuple<string, string>, double>();
			foreach (var question in ql.GetAllQuestions())
			{
				foreach (var answer in question.GetAnswers())
				{
					var from = answer.AuthorEmail;
					var to = question.AuthorEmail;
					edges.UpdateOrAdd(Tuple.Create(from,to), v => v + 1d, 1d);
				}
			}
			return edges.Select(q => Tuple.Create(q.Key.Item1,q.Key.Item2, q.Value));
		}
	}




	[TestFixture]
	internal class GraphProgram
	{
		[Test]
		public void GenerateGraph()
		{
			var g = new UserGraph(Program.DefaultQuestionList.NewQuestionListFilteredByTopics(26));
			g.ExportToPajecFormat("ggraphFlu2.net");
			
		}
		[Test]
		public void qwe(IEnumerable<string> a )
		{
			var q = Tuple.Create(5, 500);
			var w = Tuple.Create(5, 4);
			Assert.That(q, Is.LessThan(w));
		}
	}


}
