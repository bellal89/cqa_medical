using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.SpellChecker
{
	internal class SpellChecker
	{
		private readonly TrigramIndex trigramIndex;

		public SpellChecker(TrigramIndex trigramIndex)
		{
			this.trigramIndex = trigramIndex;
		}
		
		public string Fix(string word)
		{
			if (word.Length < 3) return word;
			if (trigramIndex.ContainsWord(word)) return word;

			var wordTrigrams = TrigramIndex.GetTrigramsFrom(word);

			var maxJaccard = 0.0;
			var resultWord = "";
			foreach (var candidateWord in trigramIndex.GetWordListUnion(wordTrigrams))
			{
				var candidateWordTrigrams = TrigramIndex.GetTrigramsFrom(candidateWord);
				var indexJaccard = CalculateJaccard(wordTrigrams, candidateWordTrigrams);
				if (maxJaccard >= indexJaccard) continue;
				maxJaccard = indexJaccard;
				resultWord = candidateWord;
			}
			if (maxJaccard < 0.5)
			{
				Console.WriteLine("Bad Word '{0}'. Best '{1}'", word, resultWord);
				return word;
			}
			return resultWord;
		}

		private static double CalculateJaccard(HashSet<string> wordTrigrams, HashSet<string> candidateWordTrigrams)
		{
			var intersectionCount = wordTrigrams.Intersect(candidateWordTrigrams).Count();
			return (0.0 + intersectionCount)/(wordTrigrams.Count() + candidateWordTrigrams.Count() - intersectionCount);
		}

		public static void ModifyTyposCorpus(QuestionList ql)
		{
			var detector = new SpellChecker(TrigramIndex.CreateFrom(ql));
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
		[Test, Explicit]
		public static void TestCreation()
		{
			var detector = new SpellChecker(TrigramIndex.CreateFromDefaultDictionaryAnd(Program.DefaultNotStemmedQuestionList));
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

		[Test, Explicit]
		public void TestModify()
		{
			SpellChecker.ModifyTyposCorpus(Program.DefaultNotStemmedQuestionList);
		}

		[Test]
		[TestCase("ацитил", "ацетил",1)]
		[TestCase("ацтиил", "ацетил", 2)]
		[TestCase("почему", "почиму", 1)]
		[TestCase("why", "what", 2)]
		[TestCase("why", "wyh", 1)]
		[TestCase("why", "hwy", 1)]
		public void TestLevenstein(string s1, string s2, int answer)
		{
			var levensteinInfo = new LevensteinInfo(s1, s2);
			Assert.AreEqual(answer, levensteinInfo.GetDistance());
		}

		[Test, Explicit]
		public static void TestRightWordsDictionaryCount()
		{
			var detector = TrigramIndex.CreateFrom(Program.DefaultNotStemmedQuestionList);
			Console.WriteLine(detector.IdToWord.Count);
		}

		[Test, Explicit]
		public static void FixWordTest()
		{
			string[] words = {"Проверка", "праверка"};
			var detector = new SpellChecker(TrigramIndex.CreateFromDefaultDictionaryAnd(Program.DefaultNotStemmedQuestionList));
			var fixedWords = words.Select(detector.Fix).ToArray();
			Console.WriteLine(String.Join("\n",
			                              words.Zip(fixedWords, (w1, w2) => ((w1 != w2) ? "!!!\t" : "") + w1 + "\t-\t" + w2)));
		}

		[Test]
		[TestCase("ацитил", "ацетил", "и", "е")]
		[TestCase("цаетил", "ацетил", "ца", "ац")]
		[TestCase("почему", "почиму", "е", "и")]
		[TestCase("what", "hat", "w", "")]
		[TestCase("why", "whyt", "", "t")]
		public void TestGettingMisspellings(string s1, string s2, string c1, string c2)
		{
			var levensteinInfo = new LevensteinInfo(s1, s2);
			var misspellings = levensteinInfo.GetMisspellings();
			Assert.AreEqual(misspellings[0].Item1, c1);
			Assert.AreEqual(misspellings[0].Item2, c2);
		}
	}
}
