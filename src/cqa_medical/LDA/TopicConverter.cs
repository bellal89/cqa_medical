using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.UtilitsNamespace;


namespace cqa_medical.LDA
{
	class TopicConverter
	{
		private readonly Dictionary<string, int> topicWords = new Dictionary<string, int>();
		private readonly List<Tuple<int, int, double>> topicWordChances = new List<Tuple<int, int, double>>();
		private readonly int topicsCount;

		public TopicConverter(string topicsByWordsFileName)
		{
			var i = 0;
			var nTopic = -1;
			var lines = File.ReadAllLines(topicsByWordsFileName);
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
					
					if (!topicWords.ContainsKey(word))
					{
						topicWords[word] = i++;
					}
					topicWordChances.Add(Tuple.Create(nTopic, topicWords[word], chance));
				}
			}
			topicsCount = nTopic + 1;
		}

		public void SaveTopicWordVectors(string fileToSave)
		{
			var vectors = new double[topicsCount][];
			for (int i = 0; i < topicsCount; i++)
			{
				vectors[i] = new double[topicWords.Count];
				for (int j = 0; j < topicWords.Count; j++)
				{
					vectors[i][j] = 0;
				}
			}
			foreach (var chance in topicWordChances)
			{
				vectors[chance.Item1][chance.Item2] = chance.Item3;
			}

			File.WriteAllText(fileToSave, String.Join("\n", vectors.Select(v => String.Join(" ", v.Select(d => d.ToString(CultureInfo.InvariantCulture))))));
		}

		public void SaveTopicGraph(string fileToSave, int nLabels)
		{
			var topicToWords = new Dictionary<int, List<Tuple<int, double>>>();
			foreach (var twc in topicWordChances)
			{
				if (!topicToWords.ContainsKey(twc.Item1))
				{
					topicToWords[twc.Item1] = new List<Tuple<int, double>>();
				}
				topicToWords[twc.Item1].Add(Tuple.Create(twc.Item2, twc.Item3));
			}

			var edges = (from e1 in topicToWords.Keys
			             from e2 in topicToWords.Keys
			             select Tuple.Create(e1, e2, GetWeight(topicToWords[e1], topicToWords[e2]))).ToList();

			File.WriteAllText(fileToSave,
			                  "graph Sample" + Environment.NewLine + "{" + Environment.NewLine +
							  GetLabels(topicToWords, nLabels) + Environment.NewLine +
								String.Join(Environment.NewLine,
			                        edges.Select(e => "\t" + e.Item1 + " -- " + e.Item2 + "[weight=" + e.Item3 + "]")) +
									Environment.NewLine + "}");
		}

		private string GetLabels(Dictionary<int, List<Tuple<int, double>>> topicToWords, int n)
		{
			return String.Join(Environment.NewLine,
			            topicToWords.Select(
			            	t => "\t" + t.Key + " [label=" + String.Join("_", t.Value.OrderByDescending(v => v.Item2).Take(n).Select(v => topicWords.ElementAt(v.Item1).Key)) + "]"));

		}

		private static double GetWeight(IEnumerable<Tuple<int, double>> e1, IEnumerable<Tuple<int, double>> e2)
		{
			var eDict = e2.ToDictionary(item => item.Item1, item => item.Item2);
			return e1.Where(chance => eDict.ContainsKey(chance.Item1)).Sum(chance => chance.Item2*eDict[chance.Item1]);
		}

		public IEnumerable<string> ConvertTopicToWords(int topicNumber)
		{
			Assert.Greater(topicsCount, topicNumber);
			return topicWordChances.Where(it => it.Item1 == topicNumber).OrderByDescending(it => it.Item3).Select(
				it => topicWords.ElementAt(it.Item2).Key);
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
