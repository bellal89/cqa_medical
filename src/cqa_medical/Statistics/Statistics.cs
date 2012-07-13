﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.Statistics
{
	public class StatisticsAttribute : Attribute
	{
	}

	class Statistics
    {
		private readonly Dictionary<long, Question> questionDictionary;
		private readonly IEnumerable<Question> questions;
        private readonly IEnumerable<Answer> answers;

		public Statistics(QuestionList questionList)
		{
			questionDictionary = questionList.GetQuestions();
            questions = questionDictionary.Values;
			answers = questions.SelectMany(t => t.GetAnswers());
        }
		
		
		private IEnumerable<string> SplitInWordsAndNormalize(string s)
		{
			return Regex.Split(s.StripHTMLTags(), @"\W+");
		}

		private SortedDictionary<T, int> GetDistribution<T>(IEnumerable<T> data)
		{
			return new DistributionCreator<T>(data).GetData();
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
			return GetDistribution(answers.Select(t => (int)Math.Floor((t.DateAdded - questionDictionary[t.QuestionId].DateAdded).TotalMinutes)));
		}
		[Statistics]
		public SortedDictionary<int, int> QuestionLengthDistibution()
		{
			return GetDistribution(questions.Select(t => t.Title.Length + t.Text.Length));
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
			return GetDistribution(answers.Select(a => questionDictionary[a.QuestionId].Category));
		}
		[Statistics]
		public SortedDictionary<string, int> CategoryUsersDistribution()
		{
			var categories = new HashSet<Tuple<string, string>>();
			foreach (var answer in answers)
			{
				var questionAuthor = questionDictionary[answer.QuestionId].AuthorEmail;
				var questionCategory = questionDictionary[answer.QuestionId].Category;
				categories.Add(new Tuple<string, string>(questionCategory, questionAuthor));
				categories.Add(new Tuple<string, string>(questionCategory, answer.AuthorEmail));
			}
			return GetDistribution(categories.Select(cat => cat.Item1));
		}
		[Statistics]
    	public SortedDictionary<int, int> AnswerLengthInWordsDistribution()
		{
			return GetDistribution(answers
				.Select(a => SplitInWordsAndNormalize(a.Text)
				.Where(q => q != "").ToArray().Length));
		}
		[Statistics]
		public SortedDictionary<int, int> QuestionLengthInWordsDistribution()
		{
			return GetDistribution(questions
				.Select(a => SplitInWordsAndNormalize(a.Text + a.Title)
				.Where(q => q != "").ToArray().Length));
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
			return GetDistribution(answers.Select(a => new Tuple<string, string>(questionDictionary[a.QuestionId].Category, a.AuthorEmail))
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
	}



	[TestFixture]
	 class StatisticsTest
	{
		private Statistics statistics;

		[TestFixtureSetUp]
		public void Init()
		{
			var parser = new Parser("../../Files/QuestionsTest.csv", "../../Files/AnswersTest.csv");
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
		public void TestQuestionLengthInWords()
		{
			var distibution = statistics.QuestionLengthInWordsDistribution();
			Assert.AreEqual(1, distibution[7]);
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

	}
}
