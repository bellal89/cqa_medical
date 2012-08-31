using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical;
using cqa_medical.DataInput;
using cqa_medical.Statistics;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Statistics
{
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
			var g = new Graph(Program.DefaultQuestionList);
			var adjacencyList = g.AdjacencyList;
			var allVertexes = adjacencyList.SelectMany(k => new[] {k.Key}.Concat(k.Value.Keys)).Distinct().ToList();
//			adjacencyList
//				.Where(k => k.Value.Count > 10)
//				.OrderByDescending(k => k.Value.Count)
//				.Select(k => k.FormatString())
			var nameToNumber = new VertexCoder(allVertexes).NameToNumber;
			File.WriteAllText("ggraph.net",
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
