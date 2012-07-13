using System;
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
		private readonly QuestionList questionList;
		private readonly IEnumerable<Question> questions;
        private readonly IEnumerable<Answer> answers;

		public Statistics(QuestionList questionList)
		{
			this.questionList = questionList;
            questions = questionList.GetAllQuestions();
			answers = questionList.GetAllAnswers();
        }
		
		
		private IEnumerable<string> SplitInWordsAndNormalize(string s)
		{
			return Regex.Split(s.StripHTMLTags(), @"\W+");
		}

		private DistributionCreator<T> GetDistribution<T>(IEnumerable<T> data)
		{
			return new DistributionCreator<T>(data);
		}
		[Statistics]
		public DistributionCreator<int> AnswerLengthDistibution()
		{
			return GetDistribution(answers.Select(t => t.Text.Length));
		}
		[Statistics]
		public DistributionCreator<int> AnswersAmountDistibution()
		{
			return GetDistribution(questions.Select(t => t.GetAnswers().ToArray().Length));
		}
		[Statistics]
		public DistributionCreator<int> AnswerSpeedDistibution()
		{
			return GetDistribution(answers.Select(t => (int)Math.Floor((t.DateAdded - questionList.GetQuestion(t.QuestionId).DateAdded).TotalMinutes)));
		}
		[Statistics]
		public DistributionCreator<int> QuestionLengthDistibution()
		{
			return GetDistribution(questions.Select(t => t.Title.Length + t.Text.Length));
		}
		[Statistics]
		public DistributionCreator<string> QuestionActivityInDaysDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.ToShortDateString()));
		}
		[Statistics]
		public DistributionCreator<string> AnswerActivityInDaysDistibution()
		{
			return GetDistribution(answers.Select(t => t.DateAdded.ToShortDateString()));
		}
		[Statistics]
		public DistributionCreator<string> QuestionActivityInDaysByWeekDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.DayOfWeek.ToString()));
		}
		[Statistics]
		public DistributionCreator<int> QuestionActivityInHoursByDayDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.Hour));
		}
		[Statistics]
		public DistributionCreator<string> UserActivityInMessagesDistibution()
		{
			var statisticGenerator = new DistributionCreator<string>(questions.Select(t => t.AuthorEmail));
			statisticGenerator.AddData(answers.Select(t => t.AuthorEmail));
			return statisticGenerator;
		}
		[Statistics]
		public DistributionCreator<string> CategoryQuestionsDistribution()
		{
			return GetDistribution(questions.Select(q => q.Category));
		}
		[Statistics]
		public DistributionCreator<string> CategoryAnswersDistribution()
		{
			return GetDistribution(answers.Select(a => questionList.GetQuestion(a.QuestionId).Category));
		}
		[Statistics]
		public DistributionCreator<string> CategoryUsersDistribution()
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
    	public DistributionCreator<int> AnswerLengthInWordsDistribution()
		{
			return GetDistribution(answers
				.Select(a => SplitInWordsAndNormalize(a.Text)
				.Where(q => q != "").ToArray().Length));
		}
		[Statistics]
		public DistributionCreator<int> QuestionLengthInWordsDistribution()
		{
			return GetDistribution(questions
				.Select(a => SplitInWordsAndNormalize(a.Text + a.Title)
				.Where(q => q != "").ToArray().Length));
		}
		[Statistics]
		public DistributionCreator<string> CategoryUserQuestionsDistribution()
		{
			return GetDistribution(questions.Select(q => new Tuple<string, string>(q.Category, q.AuthorEmail))
											.Distinct()
											.Select(item => item.Item1));
		}
		[Statistics]
		public DistributionCreator<string> CategoryUserAnswersDistribution()
		{
			return GetDistribution(answers.Select(a => new Tuple<string, string>(questionList.GetQuestion(a.QuestionId).Category, a.AuthorEmail))
										  .Distinct()
										  .Select(item => item.Item1));
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
			var distibution = statistics.AnswerLengthInWordsDistribution().GetData();
			Assert.AreEqual(2, distibution[3]);
			Assert.AreEqual(3, distibution[1]);
			Assert.AreEqual(2, distibution[12]);
		}

		[Test]
		public void TestQuestionLengthInWords()
		{
			var distibution = statistics.QuestionLengthInWordsDistribution().GetData();
			Assert.AreEqual(1, distibution[7]);
		}
		[Test]
		public void TestAnswerLength()
		{
			var distibution = statistics.AnswerLengthDistibution().GetData();
			Assert.AreEqual(2, distibution[8]);
			Assert.AreEqual(1, distibution[13]);
			Assert.AreEqual(2, distibution[67]);
		}

		[Test]
		public void TestAnswersAmount()
		{
			var distibution = statistics.AnswersAmountDistibution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(2, distibution[6]);
			Assert.AreEqual(1, distibution[4]);
		}
		
		[Test]
		public void TestAnswerSpeed()
		{
			var distibution = statistics.AnswerSpeedDistibution().GetData();
			Assert.AreEqual(12, distibution.Keys.ToArray().Length);
			Assert.AreEqual(3, distibution[0]);
			Assert.AreEqual(1, distibution[1]);
			Assert.AreEqual(2, distibution[2]);
			Assert.AreEqual(1, distibution[360]);
		}

		[Test]
		public void TestQuestionActivity()
		{
			var distibution = statistics.QuestionActivityInDaysDistibution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			//Assert.AreEqual(1, distibution[92]);
			//Assert.AreEqual(2, distibution[353]);
		}

		[Test]
		public void TestQuestionActivityInDaysByWeek()
		{
			var distibution = statistics.QuestionActivityInDaysByWeekDistibution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(1, distibution["Sunday"]);
			Assert.AreEqual(2, distibution["Tuesday"]);
		}

		[Test]
		public void TestQuestionActivityInHoursByDay()
		{
			var distibution = statistics.QuestionActivityInHoursByDayDistibution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(2, distibution[11]);
			Assert.AreEqual(1, distibution[22]);
		}

		[Test]
		public void TestUserActivity()
		{
			var distibution = statistics.UserActivityInMessagesDistibution().GetData();
			Assert.AreEqual(18, distibution.Keys.ToArray().Length);
		}

		[Test]
		public void TestCategoryQuestions()
		{
			var distibution = statistics.CategoryQuestionsDistribution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(1, distibution["illness"]);
			Assert.AreEqual(2, distibution["health"]);
		}

		[Test]
		public void TestCategoryAnswers()
		{
			var distibution = statistics.CategoryAnswersDistribution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(4, distibution["illness"]);
			Assert.AreEqual(12, distibution["health"]);
		}

		[Test]
		public void TestCategoryUsers()
		{
			var distibution = statistics.CategoryUsersDistribution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(5, distibution["illness"]);
			Assert.AreEqual(13, distibution["health"]);
		}

		[Test]
		public void TestCategoryUserQuestions()
		{
			var distibution = statistics.CategoryUserQuestionsDistribution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(1, distibution["illness"]);
			Assert.AreEqual(1, distibution["health"]);
		}

		[Test]
		public void TestCategoryUserAnswers()
		{
			var distibution = statistics.CategoryUserAnswersDistribution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(4, distibution["illness"]);
			Assert.AreEqual(12, distibution["health"]);
		}
	}
}
