using System;
using System.IO;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;
using cqa_medical.BodyAnalisys;

namespace cqa_medical
{
    class Program
    {

		public const string StatisticsDirectory = "../../StatOutput/";
		public const string QuestionsFileName = "../../Files/qst_25.csv";
		public const string AnswersFileName = "../../Files/ans_25.csv";

		public const string TestQuestionsFileName = "../../Files/QuestionsTest.csv";
		public const string TestAnswersFileName = "../../Files/AnswersTest.csv";

		public static QuestionList ParseAndStem()
		{
			var questionList = Parse(QuestionsFileName, AnswersFileName);
			return questionList.StemIt();
		}

		public static QuestionList Parse(String questionsFileName, string answersFileName)
    	{
    		var questionList = new QuestionList();

    		var start = DateTime.Now;
			var parser = new Parser(questionsFileName, answersFileName);
    		parser.Parse(questionList.AddQuestion, questionList.AddAnswer);

			Console.WriteLine(String.Format("Parsing Completed in {0}", (DateTime.Now - start).TotalSeconds));
    		return questionList;
    	}
		

    	public static void BodyPartsWork()
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
