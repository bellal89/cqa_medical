using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.Utilits;

namespace cqa_medical.SpellChecker
{
	internal class TrigramIndex
	{
		private readonly Dictionary<string, int> wordFrequencies;
		private Dictionary<string, HashSet<int>> trigrams;

		public TrigramIndex(QuestionList questionList)
		{
			wordFrequencies = GetDefaultWordFrequencies(questionList);
			trigrams = CalculateTrigramIndex();
		}

		private Dictionary<string, HashSet<int>> CalculateTrigramIndex()
		{
			var kgrams = new Dictionary<string, HashSet<int>>();
			for (var i = 0; i < wordFrequencies.Count; i++)
			{
				var word = wordFrequencies.ElementAt(i).Key;
				var wordTrigrams = GetKGrams(word, 3);
				foreach (var trigram in wordTrigrams)
				{
					if (!kgrams.ContainsKey(trigram))
						kgrams.Add(trigram, new HashSet<int>());
					kgrams[trigram].Add(i);
				}
			}
			return kgrams;
		}

		private static IEnumerable<string> GetKGrams(string word, int k)
		{
			var kgrams = new List<string>();
			if (k > word.Length)
			{
				return kgrams;
			}
			for (var i = 0; i < (word.Length - k + 1); i++)
			{
				kgrams.Add(word.Substring(i, k));
			}
			return kgrams;
		}

		public Dictionary<string, HashSet<int>> GetTrigrams()
		{
			return trigrams;
		}

		public Dictionary<string, int> GetWordFrequencies()
		{
			return wordFrequencies;
		}

		public static Dictionary<string, int> GetDefaultWordFrequencies(QuestionList questionList)
		{
			var getDataFunction = new Func<Tuple<string, int>[]>(() =>
			                                                     	{
			                                                     		var statistics = new Statistics.Statistics(questionList);
			                                                     		return statistics.WordFrequency(new EmptyStemmer())
			                                                     			.Where(item => item.Value > 10)
			                                                     			.Select(item => Tuple.Create(item.Key, item.Value))
			                                                     			.ToArray();
			                                                     	});

			return DataActualityChecker.Check(new Lazy<Tuple<string, int>[]>(getDataFunction),
			                                  t => t.Item1 + "\x2" + t.Item2,
			                                  s =>
			                                  	{
			                                  		var q = s.Split('\x2');
			                                  		return Tuple.Create(q[0], int.Parse(q[1]));
			                                  	},
			                                  new FileDependencies(String.Format("WordFrequencies_{0}.txt", questionList.GetHashCode()), Program.QuestionsFileName,
			                                                       Program.AnswersFileName))
				.ToDictionary(item => item.Item1, item => item.Item2);
		}

		public static Dictionary<string, HashSet<int>> GetDefaultTrigramIndex(QuestionList questionList)
		{
			var getDataFunction = new Func<Tuple<string, HashSet<int>>[]>(
				() =>
					{
						var trigramIndex = new TrigramIndex(questionList);
						return trigramIndex.CalculateTrigramIndex().Select(pair => Tuple.Create(pair.Key, pair.Value)).ToArray();
					});

			return DataActualityChecker.Check(new Lazy<Tuple<string, HashSet<int>>[]>(getDataFunction),
											  t => t.Item1 + "\x2" + String.Join("\x2", t.Item2),
											  s =>
											  {
												  var q = s.Split('\x2');
												  return Tuple.Create(q[0], new HashSet<int>(q.Skip(1).Select(int.Parse)));
											  },
											  new FileDependencies(String.Format("TrigramIndex_{0}.txt", questionList.GetHashCode()), Program.QuestionsFileName,
																   Program.AnswersFileName))
				.ToDictionary(item => item.Item1, item => item.Item2);
		}
	}

	[TestFixture]
	public class TrigramIndexTest
	{
		[Test]
		public void TestIndexCreation()
		{
			var index = new TrigramIndex(Program.DefaultNotStemmedQuestionList);
			Console.WriteLine(String.Join("\n",index.GetTrigrams().OrderByDescending(t => t.Value.Count).Select(t => t.Key + "\t" + t.Value.Count)));
		}

		[Test]
		public void TestTrigramIndexStoring()
		{
			var index = TrigramIndex.GetDefaultTrigramIndex(Program.DefaultNotStemmedQuestionList);
			Assert.AreEqual(14875, index.Count);
		}
	}

}
