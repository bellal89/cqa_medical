using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.SpellChecker
{
	internal class TrigramIndex
	{
		public SortedDictionary<int, string> WordAndId { get; private set; }
		public Dictionary<string, HashSet<int>> Trigrams { get; private set; }
		private readonly HashSet<string> vocabulary;

		public static TrigramIndex CreateFrom(QuestionList questionList)
		{
			return new TrigramIndex(questionList);
		}

		public static TrigramIndex CreateFromDefaultDictionaryAnd(QuestionList questionList)
		{
			return new TrigramIndex(questionList, Program.FilesDirectory + "1grams-3.txt");
		}


		private TrigramIndex(QuestionList questionList)
		{
			// Frequencies dictionary based on Mail.Ru corpus
			var frequencies = CalculateDefaultWordFrequencies(questionList);
			int i = 0;
			WordAndId = new SortedDictionary<int, string>(frequencies.ToDictionary(q =>  i++, q => q));
			Trigrams = GetDefaultTrigramIndex(questionList);
			vocabulary = new HashSet<string>(WordAndId.Values);
		}

		private TrigramIndex(QuestionList questionList, string wordsDictionaryFileName)
		{
			// External frequencies dictionary (Google: "ruscorpora"):
			Console.WriteLine("TrigramIndex: Geting Words from " + wordsDictionaryFileName);
			WordAndId = new SortedDictionary<int, string>(LoadFromFile(wordsDictionaryFileName));
			Console.WriteLine("TrigramIndex: Geting Trigram Index now");
			Trigrams = GetDefaultTrigramIndex(questionList);
			vocabulary = new HashSet<string>(WordAndId.Values);
		}
		public bool ContainsWord(string word)
		{
			return vocabulary.Contains(word);
		}

		private Dictionary<string, HashSet<int>> CalculateTrigramIndex()
		{
			var kgrams = new Dictionary<string, HashSet<int>>();
			for (var i = 0; i < WordAndId.Count; i++)
			{
				var word = WordAndId[i];
				var wordTrigrams = GetTrigramsFrom(word);
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
			kgrams.Add("$" + word.Substring(0, k - 1));
//			kgrams.Add(word.Substring(word.Length - k + 1, k - 1) + "$");
			return kgrams;
		}
		public static HashSet<string> GetTrigramsFrom(string word)
		{
			return GetKgramsFrom(word, 3);
		}
		private static Dictionary<int, string> LoadFromFile(string fileName)
		{
			int i = 0;
			return File.ReadAllLines(fileName)
				.Select(line => line.Split('\t'))
				.Where(a => a.Length > 1)
				.ToDictionary(a => i++, a => a[1]);
		}

		public static string[] CalculateDefaultWordFrequencies(QuestionList questionList)
		{
			var getDataFunction = new Func<string[]>(
				() =>
					{
						var statistics = new Statistics.Statistics(questionList);
						return statistics.WordFrequencyDistribution(new EmptyStemmer())
							.Where(item => item.Value > 10)
							.Select(item => item.Key)
							.ToArray();
					});

			return DataActualityChecker.Check
				(
					new Lazy<string[]>(getDataFunction),
					t => t,
					s => s,
					new FileDependencies(String.Format("FrequentWords_{0}.txt", questionList.GetHashCode()),
					                     Program.QuestionsFileName,
					                     Program.AnswersFileName)
				).ToArray();
		}

		private Dictionary<string, HashSet<int>> GetDefaultTrigramIndex(QuestionList questionList)
		{
			var getDataFunction = new Func<Tuple<string, HashSet<int>>[]>(
				() => CalculateTrigramIndex().Select(pair => Tuple.Create(pair.Key, pair.Value)).ToArray());

			return DataActualityChecker.Check
				(
					new Lazy<Tuple<string, HashSet<int>>[]>(getDataFunction),
					t => t.Item1 + "\x2" + String.Join("\x2", t.Item2),
					s =>
						{
							var q = s.Split('\x2');
							return Tuple.Create(q[0], new HashSet<int>(q.Skip(1).Select(int.Parse)));
						},
					new FileDependencies(
						String.Format("TrigramIndex_{0}.txt", questionList.GetHashCode()),
						Program.QuestionsFileName,
						Program.AnswersFileName)
				)
				.ToDictionary(item => item.Item1, item => item.Item2);
		}

		public HashSet<int> WordsIdsUnionFrom(IEnumerable<string> wordTrigrams)
		{
			var result = new HashSet<int>();
			foreach (var word in wordTrigrams.Where(Trigrams.ContainsKey).SelectMany(t=> Trigrams[t]))
				result.Add(word);
			return result;
		}
}

	[TestFixture]
	public class TrigramIndexTest
	{

		[Test, Explicit]
		public void TestIndexCreation()
		{
			var index = TrigramIndex.CreateFrom(Program.DefaultNotStemmedQuestionList);
			Console.WriteLine(String.Join("\n",
			                              index.Trigrams.OrderByDescending(t => t.Value.Count).Select(
			                              	t => t.Key + "\t" + t.Value.Count)));
		}

//		[Test]
//		public void FileNormalize()
//		{
//			const string fileName = Program.FilesDirectory + "1grams-3.txt";
//			var rawLines = File.ReadAllLines(fileName);
//			var enumerable = TrigramIndex.GetWordsAndIds(rawLines, 1, '\t');
//			File.WriteAllLines(fileName + ".modified.txt", enumerable);
//		}



		[Test, Explicit]
		public void TestIndexFromFileCreation()
		{
			var index = TrigramIndex.CreateFromDefaultDictionaryAnd(Program.DefaultNotStemmedQuestionList);
			File.WriteAllLines("right.txt",
			                   index.Trigrams.OrderByDescending(t => t.Value.Count).Select(
			                   	t => t.Key + "\t" + String.Join(" ", t.Value)));
		}
	}

}
