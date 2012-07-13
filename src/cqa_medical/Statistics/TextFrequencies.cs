using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.Statistics
{
	class TextFrequencies
	{
		private readonly string[] words;

		public TextFrequencies(string text)
		{
			words = Regex.Split(text.ToLower().StripHTMLTags(), @"\W+").Where(w => w != "").ToArray();
		}

		public Dictionary<string, int> GetOneWordDictionary()
		{
			return GetDictionary(words);
		}

		public Dictionary<Tuple<string, string>, int> GetTwoWordsDictionary()
		{
			return GetDictionary(GetAllPairs());
		}

		private IEnumerable<Tuple<string, string>> GetAllPairs()
		{
			for (var i = 0; i < words.Length - 1; i++)
				yield return Tuple.Create(words[i], words[i + 1]);
		}

		private Dictionary<T, int> GetDictionary<T>(IEnumerable<T> items)
		{
			var frequenciesDictionary = new Dictionary<T, int>();
			foreach (var word in items)
			{
				if (frequenciesDictionary.ContainsKey(word))
				{
					frequenciesDictionary[word]++;
				}
				else
				{
					frequenciesDictionary.Add(word, 1);
				}
			}
			return frequenciesDictionary;
		}
	}

	[TestFixture]
	public class QuestionListFrequenciesTest
	{
		private QuestionList questionList;

		[TestFixtureSetUp]
		public void Init()
		{
			var parser = new Parser("../../Files/qst_25.csv", "../../Files/ans_25.csv");
			questionList = new QuestionList();
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);
		}

		[Test]
		public void TestQuestionsDictionary()
		{
			var text = String.Join(" ", questionList.GetQuestions().Values.Select(q => q.Title + " " + q.Text));
			var freqs = new TextFrequencies(text);
			
//			Assert.AreEqual(2, freqs.GetOneWordDictionary()["что"]);
			Assert.AreEqual(1, freqs.GetTwoWordsDictionary()[Tuple.Create("что", "в")]);
		}

		[Test]
		public void TestAnswersDictionary()
		{
			//Assert.AreEqual(6, freqs.GetAnswersDictionary()["это"]);
			//Assert.AreEqual(2, freqs.GetAnswersDictionary()["ветеринарные"]);
		}
	}
}
