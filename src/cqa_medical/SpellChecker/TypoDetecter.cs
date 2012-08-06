using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
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
			if (trigramIndex.ContainsWord(word)) return word;

			var wordTrigrams = TrigramIndex.GetTrigramsFrom(word);

			var wordJaccards = new Dictionary<string, double>();
			foreach (var id in trigramIndex.WordsIdsUnionFrom(wordTrigrams))
			{
				var candidateWord = trigramIndex.WordAndId[id];
				var candidateWordTrigrams = TrigramIndex.GetTrigramsFrom(candidateWord);
				wordJaccards.Add(candidateWord, CalculateJaccard(wordTrigrams, candidateWordTrigrams));
			}
			if (wordJaccards.Count < 1)
			{
				Console.WriteLine("Bad Word {0}", word);
				return word;
			}
			return wordJaccards.Max(item => Tuple.Create(item.Value, item.Key)).Item2;
		}

		private static double CalculateJaccard(HashSet<string> wordTrigrams, HashSet<string> candidateWordTrigrams)
		{
			var intersectionCount = wordTrigrams.Intersect(candidateWordTrigrams).Count();
			return (0.0+intersectionCount)/(wordTrigrams.Count() + candidateWordTrigrams.Count() - intersectionCount);
		}
		public static void ModifyTyposCorpus(QuestionList ql)
		{
			var detector = new TypoDetecter(TrigramIndex.CreateFromDefaultDictionaryAnd(ql));
			Console.WriteLine("I am Modifying");

			var start = DateTime.Now;
			foreach (var question in ql.GetAllQuestions())
			{
				question.Text = String.Join(" ", question.Text.SplitInWordsAndStripHTML().Select(detector.Fix));
				question.Title = String.Join(" ", question.Title.SplitInWordsAndStripHTML().Select(detector.Fix));
			}
			Console.WriteLine("Questions modified in {0}", (DateTime.Now - start).TotalSeconds);

			start = DateTime.Now;
			foreach (var answer in ql.GetAllAnswers())
			{
				answer.Text = String.Join(" ", answer.Text.SplitInWordsAndStripHTML().Select(detector.Fix));
			}
			Console.WriteLine("Answers modified in {0}", (DateTime.Now - start).TotalSeconds);

			File.WriteAllLines(Program.QuestionsNoTyposFileName, ql.GetAllQuestions().Select(Question.FormatStringWrite));
			File.WriteAllLines(Program.AnswersNoTyposFileName, ql.GetAllAnswers().Select(Answer.FormatStringWrite));
		}
	}

	[TestFixture]
	public class TypoDetecterTest
	{
		[Test]
		public static void TestCreation()
		{
			var detector = new TypoDetecter(TrigramIndex.CreateFromDefaultDictionaryAnd(Program.DefaultNotStemmedQuestionList));
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
		public void TestModify()
		{
			TypoDetecter.ModifyTyposCorpus(Program.DefaultNotStemmedQuestionList);
		}

		[Test]
		public static void TestRightWordsDictionaryCount()
		{
			var detector = TrigramIndex.CreateFrom(Program.DefaultNotStemmedQuestionList);
			Console.WriteLine(detector.WordAndId.Count);
		}

	[Test]
		public static void FixWordTest()
	{
		string[] words = {"Проверка"};
		var detector = new TypoDetecter(TrigramIndex.CreateFromDefaultDictionaryAnd(Program.DefaultNotStemmedQuestionList));
		var fixedWords = words.Select(detector.Fix).ToArray();
		Console.WriteLine(String.Join("\n",
										  words.Zip(fixedWords, (w1, w2) => ((w1 != w2) ? "!!!\t" : "") + w1 + "\t-\t" + w2)));

	}
	}
}
