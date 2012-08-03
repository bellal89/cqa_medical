using System;
using System.Collections.Generic;
using System.IO;
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
		private readonly Dictionary<string, HashSet<int>> trigrams;

		public TrigramIndex(QuestionList questionList)
		{
			// Frequencies dictionary based on Mail.Ru corpus
			wordFrequencies = CalculateDefaultWordFrequencies(questionList);
			
			// External frequencies dictionary (Google: "ruscorpora"):
//			wordFrequencies = LoadFromFile(Program.FilesDirectory + "1grams-3.txt");
			trigrams = GetDefaultTrigramIndex(questionList);
		}

		private Dictionary<string, HashSet<int>> CalculateTrigramIndex()
		{
			var kgrams = new Dictionary<string, HashSet<int>>();
			for (var i = 0; i < wordFrequencies.Count; i++)
			{
				var word = wordFrequencies.ElementAt(i).Key;
				var wordTrigrams = GetKgramsFrom(word, 3);
				foreach (var trigram in wordTrigrams)
				{
					if (!kgrams.ContainsKey(trigram))
						kgrams.Add(trigram, new HashSet<int>());
					kgrams[trigram].Add(i);
				}
			}
			return kgrams;
		}

		public static HashSet<string> GetKgramsFrom(string word, int k)
		{
			var kgrams = new HashSet<string>();
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

		private static Dictionary<string, int> LoadFromFile(string fileName)
		{
			return File.ReadAllLines(fileName).Select(line => line.Split('\t')).Where(item => item.Length > 1).ToDictionary(
				item => item[1], item => int.Parse(item[0]));
		}

		public static Dictionary<string, int> CalculateDefaultWordFrequencies(QuestionList questionList)
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

		public Dictionary<string, HashSet<int>> GetDefaultTrigramIndex(QuestionList questionList)
		{
			var getDataFunction = new Func<Tuple<string, HashSet<int>>[]>(
				() => CalculateTrigramIndex().Select(pair => Tuple.Create(pair.Key, pair.Value)).ToArray());

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

		public IEnumerable<HashSet<int>> GetWordSetsBy(IEnumerable<string> wordTrigrams)
		{
			return wordTrigrams.Select(trigram => trigrams.ContainsKey(trigram) ? trigrams[trigram] : new HashSet<int>()).ToList();
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
	}

}
