﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Iveonik.Stemmers;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.DataInput.Stemmers.AOTLemmatizer;
using cqa_medical.DataInput.Stemmers.MyStemmer;

namespace cqa_medical.Statistics
{
	public class StatisticsAttribute : Attribute
	{
	}

	internal class Statistics
	{
		private readonly QuestionList questionList;
		private readonly Question[] questions;
		private readonly Answer[] answers;
		public static readonly DateTime FirstDate = new DateTime(2011, 9, 26);

		public Statistics(QuestionList questionList)
		{
			this.questionList = questionList;
			questions = questionList.GetAllQuestions().ToArray();
			answers = questionList.GetAllAnswers().ToArray();
		}



		public static DateTime GetWeekFromRange(DateTime now)
		{
			return FirstDate.AddDays(7*Math.Floor((now - FirstDate).TotalDays/7.0));
		}

		private SortedDictionary<T, int> GetDistribution<T>(IEnumerable<T> data)
		{
			return new DistributionCreator<T>(data).GetData();
		}

		public SortedDictionary<string, double> WordQuotientDistributionInWeeks(IEnumerable<string> expectedWords)
		{
			var enumerator = WordIntensityDistributionInWeeks(expectedWords);
			var denumerator = GetDistribution(questions
			                                  	.Where(a => a.DateAdded >= FirstDate)
			                                  	.Select(q => GetWeekFromRange(q.DateAdded).ToShortDateString()));
			return Utilits.DistributientQuotient(enumerator, denumerator);
		}

		[Statistics]
		public SortedDictionary<int, int> AnswerLengthDistibution()
		{
			return GetDistribution(answers.Select(t => t.Text.Length));
		}

		[Statistics]
		public SortedDictionary<int, int> AnswersAmountDistibution()
		{
			return GetDistribution(questions.Select(t => t.GetAnswers().ToArray().Length));
		}

		[Statistics]
		public SortedDictionary<int, int> AnswerSpeedDistibution()
		{
			return
				GetDistribution(
					answers.Select(t => (int) Math.Floor((t.DateAdded - questionList.GetQuestion(t.QuestionId).DateAdded).TotalMinutes)));
		}

		[Statistics]
		public SortedDictionary<int, int> QuestionLengthDistibution()
		{
			return GetDistribution(questions.Select(t => t.WholeText.Length));
		}

		[Statistics]
		public SortedDictionary<string, int> QuestionActivityInDaysDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.ToShortDateString()));
		}

		[Statistics]
		public SortedDictionary<string, int> AnswerActivityInDaysDistibution()
		{
			return GetDistribution(answers.Select(t => t.DateAdded.ToShortDateString()));
		}

		[Statistics]
		public SortedDictionary<string, int> QuestionActivityInDaysByWeekDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.DayOfWeek.ToString()));
		}

		[Statistics]
		public SortedDictionary<int, int> QuestionActivityInHoursByDayDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.Hour));
		}

		[Statistics]
		public SortedDictionary<string, int> UserActivityInQuestionsDistibution()
		{
			return GetDistribution(questions.Select(t => t.AuthorEmail));
		}

		[Statistics]
		public SortedDictionary<string, int> UserActivityInAnswersDistibution()
		{
			return GetDistribution(answers.Select(t => t.AuthorEmail));
		}

		[Statistics]
		public SortedDictionary<string, int> UserActivityInMessagesDistibution()
		{
			var statisticGenerator = new DistributionCreator<string>(questions.Select(t => t.AuthorEmail));
			statisticGenerator.AddData(answers.Select(t => t.AuthorEmail));
			return statisticGenerator.GetData();
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryQuestionsDistribution()
		{
			return GetDistribution(questions.Select(q => q.Category));
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryAnswersDistribution()
		{
			return GetDistribution(answers.Select(a => questionList.GetQuestion(a.QuestionId).Category));
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryUsersDistribution()
		{
			var categories = new HashSet<Tuple<string, string>>();
			foreach (var answer in answers)
			{
				var question = questionList.GetQuestion(answer.QuestionId);
				categories.Add(new Tuple<string, string>(question.Category, question.AuthorEmail));
				categories.Add(new Tuple<string, string>(question.Category, answer.AuthorEmail));
			}
			return GetDistribution(categories.Select(cat => cat.Item1));
		}

		[Statistics]
		public SortedDictionary<int, int> AnswerLengthInWordsDistribution()
		{
			return GetDistribution(answers
			                       	.Select(a => a.Text.SplitInWordsAndStripHTML().ToArray().Length));
		}

		[Statistics]
		public SortedDictionary<int, int> QuestionLengthInWordsDistribution()
		{
			return GetDistribution(questions
			                       	.Select(a => a.WholeText.SplitInWordsAndStripHTML().ToArray().Length));
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryUserQuestionsDistribution()
		{
			return GetDistribution(questions.Select(q => new Tuple<string, string>(q.Category, q.AuthorEmail))
			                       	.Distinct()
			                       	.Select(item => item.Item1));
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryUserAnswersDistribution()
		{
			return
				GetDistribution(answers.Select(
					a => new Tuple<string, string>(questionList.GetQuestion(a.QuestionId).Category, a.AuthorEmail))
				                	.Distinct()
				                	.Select(item => item.Item1));
		}

		/// <summary>
		/// This distribution is statistics of another type
		/// </summary>
		/// <returns>{user => (questionsAmount, answersAmount)}</returns>
		[Statistics]
		public Dictionary<string, Tuple<int, int>> QuestionsAnswersUserDistribution()
		{
			SortedDictionary<string, int> questionsActivity = UserActivityInQuestionsDistibution();
			SortedDictionary<string, int> answersActivity = UserActivityInAnswersDistibution();
			var userNames = questionsActivity.Keys.Union(answersActivity.Keys);
			return userNames.ToDictionary(
				userName => userName,
				userName => Tuple.Create(questionsActivity.GetOrDefault(userName, 0), answersActivity.GetOrDefault(userName, 0)));
		}

		[Statistics]
		public SortedDictionary<int, int> QuestionsAmountPerUserDistribution()
		{
			return GetDistribution(questions.GroupBy(q => q.AuthorEmail, (email, qs) => qs.Count()));
		}

		[Statistics]
		public SortedDictionary<int, int> AnswersAmountPerUserDistribution()
		{
			return GetDistribution(answers.GroupBy(a => a.AuthorEmail, (email, qs) => qs.Count()));
		}

		public SortedDictionary<string, int> WordIntensityDistributionInWeeks(IEnumerable<string> expectedWords)
		{
			var words = expectedWords.Select(s => s.ToLower());
			return GetDistribution(questions
			                       	.Where(a => a.DateAdded >= FirstDate)
			                       	.Where(q =>
			                       	       q.WholeText.SplitInWordsAndStripHTML().Any(t => words.Any(z => z == t))
			                       	       ||
			                       	       q.GetAnswers()
			                       	       	.Any(a => a.Text.SplitInWordsAndStripHTML()
			                       	       	          	.Select(w => w.ToLower())
			                       	       	          	.Any(textWord => words.Any(expectedWord => expectedWord == textWord))))
			                       	.Select(q => GetWeekFromRange(q.DateAdded).ToShortDateString()));
		}

		public SortedDictionary<string, int> WordFrequency(IStemmer stemmer)
		{
			var statisticGenerator =
				new DistributionCreator<string>(
					questions.SelectMany(t => t.WholeText.SplitInWordsAndStripHTML()).Select(stemmer.Stem));
			statisticGenerator.AddData(answers.SelectMany(t => t.Text.SplitInWordsAndStripHTML()).Select(stemmer.Stem));
			return statisticGenerator.GetData();
		}
	}



	[TestFixture]
	internal class StatisticsTest
	{
		private Statistics statistics;

		[TestFixtureSetUp]
		public void Init()
		{
			var parser = new Parser(Program.TestQuestionsFileName, Program.TestAnswersFileName);
			var questionList = new QuestionList();
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);
			statistics = new Statistics(questionList);
		}


		[Test]
		public void TestAnswerLengthInWords()
		{
			var distibution = statistics.AnswerLengthInWordsDistribution();
			Assert.AreEqual(2, distibution[3]);
			Assert.AreEqual(3, distibution[1]);
			Assert.AreEqual(2, distibution[12]);
		}

		[Test]
		public void TestGetWeekFromRange()
		{
			Assert.AreEqual(Statistics.FirstDate, Statistics.GetWeekFromRange(Statistics.FirstDate.AddDays(1)));
			Assert.AreEqual(Statistics.FirstDate, Statistics.GetWeekFromRange(Statistics.FirstDate.AddDays(2)));
			Assert.AreEqual(Statistics.FirstDate, Statistics.GetWeekFromRange(Statistics.FirstDate.AddDays(3)));
			Assert.AreEqual(Statistics.FirstDate, Statistics.GetWeekFromRange(Statistics.FirstDate.AddDays(4)));
			Assert.AreEqual(Statistics.FirstDate.AddDays(7), Statistics.GetWeekFromRange(Statistics.FirstDate.AddDays(7)));
			Assert.AreEqual(Statistics.FirstDate.AddDays(7), Statistics.GetWeekFromRange(Statistics.FirstDate.AddDays(8)));
		}

		[Test]
		public void TestQuestionLengthInWords()
		{
			var distibution = statistics.QuestionLengthInWordsDistribution();
			Console.WriteLine(distibution.ToStringNormal());
			Assert.AreEqual(1, distibution[8]);
		}

		[Test]
		public void TestAnswerLength()
		{
			var distibution = statistics.AnswerLengthDistibution();
			Assert.AreEqual(2, distibution[8]);
			Assert.AreEqual(1, distibution[13]);
			Assert.AreEqual(2, distibution[67]);
		}

		[Test]
		public void TestAnswersAmount()
		{
			var distibution = statistics.AnswersAmountDistibution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(2, distibution[6]);
			Assert.AreEqual(1, distibution[4]);
		}

		[Test]
		public void TestAnswerSpeed()
		{
			var distibution = statistics.AnswerSpeedDistibution();
			Assert.AreEqual(12, distibution.Keys.ToArray().Length);
			Assert.AreEqual(3, distibution[0]);
			Assert.AreEqual(1, distibution[1]);
			Assert.AreEqual(2, distibution[2]);
			Assert.AreEqual(1, distibution[360]);
		}

		[Test]
		public void TestQuestionActivity()
		{
			var distibution = statistics.QuestionActivityInDaysDistibution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			//Assert.AreEqual(1, distibution[92]);
			//Assert.AreEqual(2, distibution[353]);
		}

		[Test]
		public void TestQuestionActivityInDaysByWeek()
		{
			var distibution = statistics.QuestionActivityInDaysByWeekDistibution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(1, distibution["Sunday"]);
			Assert.AreEqual(2, distibution["Tuesday"]);
		}

		[Test]
		public void TestQuestionActivityInHoursByDay()
		{
			var distibution = statistics.QuestionActivityInHoursByDayDistibution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(2, distibution[11]);
			Assert.AreEqual(1, distibution[22]);
		}

		[Test]
		public void TestUserActivity()
		{
			var distibution = statistics.UserActivityInMessagesDistibution();
			Assert.AreEqual(15, distibution.Keys.ToArray().Length);
		}

		[Test]
		public void TestCategoryQuestions()
		{
			var distibution = statistics.CategoryQuestionsDistribution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(1, distibution["illness"]);
			Assert.AreEqual(2, distibution["health"]);
		}

		[Test]
		public void TestCategoryAnswers()
		{
			var distibution = statistics.CategoryAnswersDistribution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(4, distibution["illness"]);
			Assert.AreEqual(12, distibution["health"]);
		}

		[Test]
		public void TestCategoryUsers()
		{
			var distibution = statistics.CategoryUsersDistribution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(5, distibution["illness"]);
			Assert.AreEqual(11, distibution["health"]);
		}

		[Test]
		public void TestCategoryUserQuestions()
		{
			var distibution = statistics.CategoryUserQuestionsDistribution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(1, distibution["illness"]);
			Assert.AreEqual(1, distibution["health"]);
		}

		[Test]
		public void TestCategoryUserAnswers()
		{
			var distibution = statistics.CategoryUserAnswersDistribution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(4, distibution["illness"]);
			Assert.AreEqual(11, distibution["health"]);
		}

		[Test]
		public void TestQuestionsAnswersUserActivity()
		{
			var distibution = statistics.QuestionsAnswersUserDistribution();
			Assert.AreEqual(15, distibution.Keys.ToArray().Length);
			Assert.AreEqual(Tuple.Create(2, 3), distibution["zaya2802@mail.ru"]);
		}

		[Test]
		public void TestQuestionsPerUser()
		{
			var distibution = statistics.QuestionsAmountPerUserDistribution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(1, distibution[1]);
			Assert.AreEqual(1, distibution[2]);
		}

		[Test]
		public void TestAnswersPerUser()
		{
			var distibution = statistics.AnswersAmountPerUserDistribution();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(13, distibution[1]);
			Assert.AreEqual(1, distibution[3]);
		}
	}


	[TestFixture]
	internal class GetDistributions
	{
		private static Statistics statistics;

		[TestFixtureSetUp]
		public void DistributionInit()
		{
			var ql = Program.Parse(Program.QuestionsFileName, Program.AnswersFileName);
			statistics = new Statistics(ql);
		}

		[Test]
		public void CommonStatistics()
		{
			IEnumerable<MethodInfo> infos = statistics
				.GetType()
				.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(m => m.GetCustomAttributes(typeof (StatisticsAttribute), true).Any()).ToList();

			var rawMethod = typeof (Utilits).GetMethod("ToStringNormal");
			foreach (var info in infos)
			{
				Console.WriteLine("calculating " + info.Name);
				var data = info.Invoke(statistics, new object[0]);
				var genericMethod = rawMethod.MakeGenericMethod(data.GetType().GetGenericArguments());
				var outString = genericMethod.Invoke(null, new[] {data});
				File.WriteAllText(Program.StatisticsDirectory + info.Name + ".txt", (string) outString);
			}
		}


		[Test, TestCaseSource("DivideCases")]
		public void WordQuotientDistributionInWeeks(string[] expectedWords)
		{
			Console.WriteLine("calculating WordQuotientDistributionInWeeks, words: " + String.Join(", ", expectedWords));
			var data = statistics.WordQuotientDistributionInWeeks(expectedWords).ToStringNormal();
			File.WriteAllText(
				Program.StatisticsDirectory + "WordQuotientDistributionInWeeks_" + String.Join("_", expectedWords) + ".txt", data);
		}

		[Test, TestCaseSource("DivideCases")]
		public void WordIntensityDistributionInWeeks(string[] expectedWords)
		{
			Console.WriteLine("calculating WordIntensityDistributionInWeeks, words: " + String.Join(", ", expectedWords));
			var data = statistics.WordIntensityDistributionInWeeks(expectedWords).ToStringNormal();
			File.WriteAllText(
				Program.StatisticsDirectory + "WordIntensityDistributionInWeeks_" + String.Join("_", expectedWords) + ".txt", data);
		}

		private static object[] divideCases = new object[]
		                                      	{
		                                      		new object[] {new[] {"грипп", "ОРВИ"}}
		                                      	};

		[Test]
		public void WordFrequency()
		{
			foreach (var stemmer in StemmerCases)
			{
				Console.WriteLine("calculating WordFrequency_" + stemmer);
				var data = statistics.WordFrequency(stemmer).ToStringNormal();
				File.WriteAllText(
					Program.StatisticsDirectory + "WordFrequency_" + stemmer + ".txt", data);
			}
		}
		private static readonly IStemmer[] StemmerCases = {
		                                      		new RussianStemmer(),
													//new MyStemmer(null), //TODO
 													new AOTLemmatizer() 
		                                      	};
	}
}
