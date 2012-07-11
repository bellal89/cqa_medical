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
        private readonly IEnumerable<Question> questions;
        private readonly IEnumerable<Answer> answers;

		public Statistics(QuestionList questionList)
        {
            this.questions = questionList.GetQuestions().Values;
			this.answers = questions.SelectMany(t => t.GetAnswers());
        }

		public string AnswerLengthDistibution()
		{
			var data = answers.Select(t => t.Text.Length);
			var statisticGenerator = new DistributionCreator<int>(data);
			return statisticGenerator.ToString();
		}


    	public void SaveResultsToFile(string filename, string text)
        {
			using (var writer = new StreamWriter(new FileStream(filename, FileMode.Create), Encoding.GetEncoding(1251)))
			{
				writer.WriteLine(text);	
			}
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
