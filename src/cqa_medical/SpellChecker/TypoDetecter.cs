using System;
using System.Collections.Generic;
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
		public static IEnumerable<Tuple<string,int>> GetDefault()
		{
			return DataActualityChecker.Check(new Lazy<Tuple<string, int>[]>(() =>
			                                                                 	{
			                                                                 		var typoDetecter =
																						new TypoDetecter(Program.DefaultNotStemmedQuestionList);
			                                                                 		return typoDetecter
																						.GetWordFrequencies()
																						.Where(item => item.Value > 10)
																						.OrderByDescending(item => item.Value)
																						.Select(item => Tuple.Create(item.Key, item.Value))
																						.ToArray();
			                                                                 	}),
																				t => t.Item1 + "\t" + t.Item2,
			                                  s =>
			                                  	{
			                                  		var q = s.Split('\t');
			                                  		return Tuple.Create(q[0], int.Parse(q[1]));
			                                  	},
											  new FileDependencies("GetWordFrequencies.txt", Program.QuestionsFileName, Program.AnswersFileName));



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
			Console.WriteLine("Go!");
			var start = DateTime.Now;
			var enumerable = TypoDetecter.GetDefault();
			File.WriteAllText(Program.StatisticsDirectory + "DirtyWordFrequenciesFromUpTo10.txt",
				String.Join("\n", enumerable.ToArray().Select(s => s.Item1 + "\t" + s.Item2)));

			Console.WriteLine("WordFrequencies created in {0} seconds", (DateTime.Now - start).TotalSeconds);
		}
	}
}
