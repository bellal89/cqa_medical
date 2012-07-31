using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.SpellChecker
{
	class TrigramIndex
	{
		private readonly Dictionary<string, int> wordFrequencies;
		private Dictionary<string, HashSet<int>> trigrams;
 
		public TrigramIndex(QuestionList questionList)
		{
			var statistics = new Statistics.Statistics(questionList);
			wordFrequencies = statistics.WordFrequency(new EmptyStemmer())
										.Where(item => item.Value > 10)
										.ToDictionary(item => item.Key, item => item.Value);

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
			return kgrams;
		}

		public Dictionary<string, int> GetWordFrequencies()
		{
			return wordFrequencies;
		}



	}

	[TestFixture]
	public class TrigramIndexTest
	{
		[Test]
		public void TestIndexCreation()
		{
			var index = new TrigramIndex(Program.DefaultNotStemmedQuestionList);
			index.GetWordFrequencies();
		}
	}

}
