using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.Utilits;

namespace cqa_medical.SpellChecker
{
	class TypoDetecter
	{
		private readonly TrigramIndex trigramIndex;
	
		public TypoDetecter(TrigramIndex trigramIndex)
		{
			this.trigramIndex = trigramIndex;
		}

		public string Fix(string word)
		{
			if (word.Length < 3) return word;
			if (trigramIndex.GetWordFrequencies().ContainsKey(word)) return word;

			HashSet<string> wordTrigrams = TrigramIndex.GetKgramsFrom(word, 3);
			var wordSets = trigramIndex.GetWordSetsBy(wordTrigrams);

			var wordJaccards = new Dictionary<string, double>();
			foreach (var id in wordSets.SelectMany(set => set).Distinct())
			{
				var candidateWord = trigramIndex.GetWordFrequencies().ElementAt(id).Key;
				var candidateWordTrigrams = TrigramIndex.GetKgramsFrom(candidateWord, 3);
				wordJaccards.Add(candidateWord, CalculateJaccard(wordTrigrams, candidateWordTrigrams));
			}
			return wordJaccards.Max(item => Tuple.Create(item.Value, item.Key)).Item2;
		}

		private static double CalculateJaccard(HashSet<string> wordTrigrams, HashSet<string> candidateWordTrigrams)
		{
			var intersectionCount = wordTrigrams.Intersect(candidateWordTrigrams).Count();
			return (0.0+intersectionCount)/(wordTrigrams.Count() + candidateWordTrigrams.Count() - intersectionCount);
		}
	}

	[TestFixture]
	public class TypoDetecterTest
	{
		[Test]
		public static void TestCreation()
		{
			var detector = new TypoDetecter(new TrigramIndex(Program.DefaultNotStemmedQuestionList));
			Console.WriteLine("Now we can fix:");
			var words =
				Program.DefaultNotStemmedQuestionList.GetAllQuestions().Take(10).SelectMany(
					item => item.WholeText.SplitInWordsAndStripHTML()).ToArray();
			var start = DateTime.Now;
			var fixedWords = words.Select(detector.Fix).ToArray();
			Console.WriteLine("For {0} sec to {1} words:\n{2}", (DateTime.Now - start).TotalSeconds, fixedWords.Length,
			                  String.Join("\n",
			                              words.Zip(fixedWords, (w1, w2) => ((w1 != w2) ? "!!!\t" : "") + w1 + "\t-\t" + w2)));
		}

		[Test]
		public static void TestRightWordsDictionaryCount()
		{
			var detector = new TrigramIndex(Program.DefaultNotStemmedQuestionList);
			Console.WriteLine(detector.GetWordFrequencies().Count);
		}
	}
}
