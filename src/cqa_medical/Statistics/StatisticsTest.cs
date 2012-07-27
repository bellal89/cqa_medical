using System;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.Utilits;

namespace cqa_medical.Statistics
{
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
}