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
	class TypoDetecter
	{
		private readonly SortedDictionary<string, int> rightWordFrequencies;
//		private readonly TrigramIndex
	
		public TypoDetecter(QuestionList questionList)
		{
			var statistics = new Statistics.Statistics(questionList);
			rightWordFrequencies = statistics.WordFrequency(new EmptyStemmer());
		}

		public SortedDictionary<string, int> GetWordFrequencies()
		{
			return rightWordFrequencies;
		}

//		public string Fix(string word)
//		{
//			if (rightWordFrequencies.ContainsKey(word)) return word;
//			var trigrams = GetTrigrams(word);
//		}

	}

	[TestFixture]
	public class TypoDetecterTest
	{
		[Test]
		public static void TestCreation()
		{			
			var questionList = Program.Parse(Program.QuestionsFileName, Program.AnswersFileName);
			Console.WriteLine("Go!");
			var start = DateTime.Now;
			var typoDetecter = new TypoDetecter(questionList);
			
			File.WriteAllText(Program.StatisticsDirectory + "DirtyWordFrequenciesFromUpTo10.txt",
				String.Join("\n",
							  typoDetecter.GetWordFrequencies().Where(item => item.Value > 10).OrderByDescending(item => item.Value).Select(
								item => item.Key + "\t" + item.Value)));

			Console.WriteLine("WordFrequencies created in {0} seconds", (DateTime.Now - start).TotalSeconds);
		}
	}
}
