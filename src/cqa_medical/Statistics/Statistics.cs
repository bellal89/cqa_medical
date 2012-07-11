using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
			this.questionDictionary = questionList.GetQuestions();
            this.questions = questionDictionary.Values;
			this.answers = questions.SelectMany(t => t.GetAnswers());
        }

		private string GetDistribution<T>(IEnumerable<T> data)
		{
			var statisticGenerator = new DistributionCreator<T>(data);
			return statisticGenerator.ToString();
		}

		public void SaveResultsToFile(string filename, string text)
		{
			using (var writer = new StreamWriter(new FileStream(filename, FileMode.Create), Encoding.GetEncoding(1251)))
			{
				writer.WriteLine(text);
			}
		}

		public string AnswerLengthDistibution()
		{
			return GetDistribution(answers.Select(t => t.Text.Length));
		}
		public string AnswerAmountDistibution()
		{
			return GetDistribution(questions.Select(t => t.GetAnswers().ToArray().Length));
		}
		public string AnswerSpeedDistibution()
		{
			return GetDistribution(answers.Select(t => t.DateAdded - questionDictionary[t.QuestionId].DateAdded));
		}
		public string QuestionLengthDistibution()
		{
			return GetDistribution(questions.Select(t => t.Title.Length + t.Text.Length));
		}
		public string QuestionActivityInTimeDistibution()
		{
			var d = new DateTime(2000, 1, 1);
			return GetDistribution(questions.Select(t =>(t.DateAdded - d).TotalDays));
				        	
		}
		public string UserActivityInMessagesDistibution()
		{
			var statisticGenerator = new DistributionCreator<string>(questions.Select(t => t.AuthorEmail));
			statisticGenerator.AddData(answers.Select(t => t.AuthorEmail));
			return statisticGenerator.ToString();
		}

    	public string CategoryQuestionsDistribution()
		{
			return GetDistribution(questions.Select(q => q.Category));
		}

		public string CategoryAnswersDistribution()
		{
			return GetDistribution(answers.Select(a => questionDictionary[a.QuestionId].Category));
		}



    }

	[TestFixture]
	 class AnswerLengthStatisticCreatorTest
	{
		[Test]
		public void TestIt()
		{
			var parser = new Parser("../../Files/QuestionsTest.csv", "../../Files/AnswersTest.csv");
			var questionList = new QuestionList();
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);
			var statistics = new Statistics(questionList);
				statistics.SaveResultsToFile("1.txt", statistics.AnswerLengthDistibution());

		}
	}
}
