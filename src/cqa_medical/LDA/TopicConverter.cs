using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.Statistics;


namespace cqa_medical.LDA
{
	class TopicConverter
	{
		private readonly Dictionary<string, int> words = new Dictionary<string, int>();
		private readonly Dictionary<int, List<Tuple<int, double>>> topicToWords = new Dictionary<int, List<Tuple<int, double>>>();
		private readonly int topicsCount;

		public TopicConverter(string topicsByWordsFileName)
		{
			var i = 0;
			var nTopic = -1;
			var lines = File.ReadAllLines(topicsByWordsFileName, Encoding.UTF8);
			foreach(var line in lines)
			{
				if(line.StartsWith("Topic"))
				{
					nTopic++;
				}
				else
				{
					var parts = line.Split(new[]{'\t', ' '}, StringSplitOptions.RemoveEmptyEntries);
					var word = parts[0];
					var chance = double.Parse(parts[1], CultureInfo.InvariantCulture);
					
					if (!words.ContainsKey(word))
					{
						words[word] = i++;
					}
					
					if(!topicToWords.ContainsKey(nTopic))
						topicToWords[nTopic] = new List<Tuple<int, double>>();
					topicToWords[nTopic].Add(Tuple.Create(words[word], chance));
				}
			}
			topicsCount = nTopic + 1;
		}

		public void SaveTopicWordVectors(string fileToSave)
		{
			var vectors = new double[topicsCount][];
			
			for (var i = 0; i < topicsCount; i++)
			{
				vectors[i] = new double[words.Count];
				vectors[i].Initialize();
			}
			
			foreach (var nTopic in topicToWords.Keys)
			{
				foreach (var wordChance in topicToWords[nTopic])
				{
					vectors[nTopic][wordChance.Item1] = wordChance.Item2;
				}
			}

			File.WriteAllText(fileToSave, String.Join("\n", vectors.Select(v => String.Join(" ", v.Select(d => d.ToString(CultureInfo.InvariantCulture))))));
		}

		public void SaveTopicGraph(string fileToSave, int nWords)
		{
			var vertices = topicToWords.Select(t => new Vertex {Id = t.Key, Label = String.Join(", ", GetTopicWords(t.Key, nWords)), Weight = 1});
			var edges = from e1 in topicToWords.Keys 
						from e2 in topicToWords.Keys 
						select new Edge {SourceId = e1, DestinationId = e2, Weight = GetWeight(topicToWords[e1], topicToWords[e2])};

			var graph = new GraphBuilder(vertices, edges);
			graph.ExportToGVFormat(fileToSave, "TopicGraph", isOriented:false);
		}

		private static double GetWeight(IEnumerable<Tuple<int, double>> e1, IEnumerable<Tuple<int, double>> e2)
		{
			var eDict = e2.ToDictionary(item => item.Item1, item => item.Item2);
			return e1.Where(chance => eDict.ContainsKey(chance.Item1)).Sum(chance => chance.Item2*eDict[chance.Item1]);
		}

		public IEnumerable<string> GetTopicWords(int topicNumber, int nWords)
		{
			Assert.Greater(topicsCount, topicNumber);
			return topicToWords[topicNumber]
					.OrderByDescending(t => t.Item2)
					.Take(nWords)
					.Select(t => words.ElementAt(t.Item1).Key);
		}
	}

	[TestFixture]
	public class TopicconverterTest
	{
		[Test]
		public static void TestConvertion()
		{
			var converter = new TopicConverter(@"c:\Users\beloborodov\Documents\GibbsLDA\GibbsLDA++-0.2\CQA_LDA\model-00500.twords");
			converter.SaveTopicWordVectors(@"c:\Users\beloborodov\Documents\GibbsLDA\GibbsLDA++-0.2\CQA_LDA\model-00500.vectors");
		}

		[Test]
		public static void TestGraphGeneration()
		{
			var converter = new TopicConverter(@"c:\Users\beloborodov\Documents\GibbsLDA\GibbsLDA++-0.2\CQA_LDA\model-final-100_topics-100_iters.twords");
			converter.SaveTopicGraph(@"c:\Users\beloborodov\Documents\GibbsLDA\GibbsLDA++-0.2\CQA_LDA\model-00700.gv", 10);
		}

	}
}
