using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.Statistics
{
    class AnswerLengthStatisticCreator
    {
        private readonly IEnumerable<Question> questions;
		public AnswerLengthStatisticCreator(IEnumerable<Question> questions)
        {
            this.questions = questions;
        }
        public void SaveResultsToFile(string filename)
        {
        	var data = questions.SelectMany(t => t.GetAnswers()).Select(t => t.Text.Length);
			var statisticGenerator = new DistributionCreator<int>(data);
			using (var writer = new StreamWriter(new FileStream(filename, FileMode.Create), Encoding.GetEncoding(1251)))
			{
				writer.WriteLine(statisticGenerator);	
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

			new AnswerLengthStatisticCreator(questionList.GetQuestions().Values).SaveResultsToFile("1.txt");

		}
	}
}
