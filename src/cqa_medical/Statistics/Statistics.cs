using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.Statistics
{
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

		private DistributionCreator<T> GetDistribution<T>(IEnumerable<T> data)
		{
			return new DistributionCreator<T>(data);
		}

		public void SaveResultsToFile(string filename, string text)
		{
			using (var writer = new StreamWriter(new FileStream(filename, FileMode.Create), Encoding.GetEncoding(1251)))
			{
				writer.WriteLine(text);
			}
		}

		public DistributionCreator<int> AnswerLengthDistibution()
		{
			return GetDistribution(answers.Select(t => t.Text.Length));
		}

		public DistributionCreator<int> AnswersAmountDistibution()
		{
			return GetDistribution(questions.Select(t => t.GetAnswers().ToArray().Length));
		}

		public DistributionCreator<int> AnswerSpeedDistibution()
		{
			return GetDistribution(answers.Select(t => (int)Math.Floor((t.DateAdded - questionDictionary[t.QuestionId].DateAdded).TotalMinutes)));
		}

		public DistributionCreator<int> QuestionLengthDistibution()
		{
			return GetDistribution(questions.Select(t => t.Title.Length + t.Text.Length));
		}

		public DistributionCreator<int> QuestionActivityInTimeDistibution()
		{
			var d = new DateTime(2011, 1, 1);
			return GetDistribution(questions.Select(t => (int)Math.Floor((t.DateAdded - d).TotalDays)));
				        	
		}

		public DistributionCreator<string> UserActivityInMessagesDistibution()
		{
			var statisticGenerator = new DistributionCreator<string>(questions.Select(t => t.AuthorEmail));
			statisticGenerator.AddData(answers.Select(t => t.AuthorEmail));
			return statisticGenerator;
		}

		public DistributionCreator<string> CategoryQuestionsDistribution()
		{
			return GetDistribution(questions.Select(q => q.Category));
		}

		public DistributionCreator<string> CategoryAnswersDistribution()
		{
			return GetDistribution(answers.Select(a => questionDictionary[a.QuestionId].Category));
		}

		public DistributionCreator<string> CategoryUsersDistribution()
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

		private string DeleteHTMLTagsFrom(string s)
		{
			return Regex.Replace(s, "<[^>]*?>", string.Empty, RegexOptions.IgnoreCase);
		}
		private string[] SplitInWordsAndNormilize(string s)
		{
			return Regex.Split(DeleteHTMLTagsFrom(s), @"\W+");
		}
		

    	public DistributionCreator<int> AnswerLengthInWordsDistribution()
		{
			return GetDistribution(answers
				.Select(a => SplitInWordsAndNormilize(a.Text)
				.Where(q => q != "").ToArray().Length));
		}
		public DistributionCreator<int> QuestionLengthInWordsDistribution()
		{
			return GetDistribution(questions
				.Select(a => SplitInWordsAndNormilize(a.Text + a.Title)
				.Where(q => q != "").ToArray().Length));
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
			var distibution = statistics.QuestionActivityInTimeDistibution().GetData();
			Assert.AreEqual(2, distibution.Keys.ToArray().Length);
			Assert.AreEqual(1, distibution[92]);
			Assert.AreEqual(2, distibution[353]);
		}
		
		[Test]
		public void TestUserActivity()
		{
			var distibution = statistics.UserActivityInMessagesDistibution().GetData();
			Assert.AreEqual(19, distibution.Keys.ToArray().Length);
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
			Assert.AreEqual(14, distibution["health"]);
		}
	}
}
