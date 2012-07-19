using System;
using System.IO;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;
using cqa_medical.Statistics;
using cqa_medical.BodyAnalisys;

namespace cqa_medical
{
    class Program
    {
		private static QuestionList ParseAndStem()
		{
			const string questionsFileName = "../../Files/qst_25.csv";
			const string answersFileName = "../../Files/ans_25.csv";
			const string statisticsDirectory = "../../StatOutput/";
			var questionList = new QuestionList();
			DateTime start;

			start = DateTime.Now;
			var parser = new Parser(questionsFileName, answersFileName);
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);


			Console.WriteLine(String.Format("Parsing Completed in {0}", (DateTime.Now - start).TotalSeconds));
			return questionList.StemIt();
		}


        static void Main(string[] args)
        {
        	var questionList = ParseAndStem();
        	var body = BodyPart.GetBodyPartsFromFile(@"..\..\Files\BodyParts.txt");
			var calc = new BodyCalculator(questionList, body);
			calc.CalculateQuestionDistribution();
			var newBody = calc.GetBody();
        	var allQuestionsCount = questionList.GetAllQuestions().Count();
        	File.WriteAllText("1.txt", newBody.ToExcelString(allQuestionsCount), Encoding.UTF8);
			File.WriteAllText("2.txt", newBody.ToString(allQuestionsCount), Encoding.UTF8);
        }

    	
    }
}
