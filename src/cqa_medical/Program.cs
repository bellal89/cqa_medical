using System;
using System.IO;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;
using cqa_medical.Statistics;
using cqa_medical.BodyAnalisys;
using cqa_medical.DataInput.AOTLemmatizer;

namespace cqa_medical
{
    class Program
    {

		const string StatisticsDirectory = "../../StatOutput/";
		const string QuestionsFileName = "../../Files/qst_25.csv";
		const string AnswersFileName = "../../Files/ans_25.csv";

		const string QuestionsTestFileName = "../../Files/QuestionsTest.csv";
		const string AnswersTestFileName = "../../Files/AnswersTest.csv";

		private static QuestionList ParseAndStem()
		{
			var questionList = Parse(QuestionsFileName, AnswersFileName);
			return questionList.StemIt();
		}

		private static QuestionList Parse(String questionsFileName, string answersFileName)
    	{
    		var questionList = new QuestionList();

    		var start = DateTime.Now;
			var parser = new Parser(questionsFileName, answersFileName);
    		parser.Parse(questionList.AddQuestion, questionList.AddAnswer);

			Console.WriteLine(String.Format("Parsing Completed in {0}", (DateTime.Now - start).TotalSeconds));
    		return questionList;
    	}
		

    	private static void BodyPartsWork()
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

		static void Main(string[] args)
		{
			//var questionList = Parse(QuestionsFileName, AnswersFileName);
			//var statistics = new Statistics.Statistics(questionList);
			//File.WriteAllText(
			//    "WordIntensityDistributionInWeeks.txt",
			//    statistics.WordIntensityDistributionInWeeks(new string[] { "грипп", "ОРВИ" }).ToStringNormal()
			//);
			//File.WriteAllText(
			//    "WordQuotientDistributionInWeeks.txt",
			//    statistics.WordQuotientDistributionInWeeks(new string[] { "грипп", "ОРВИ" }).ToStringNormal()
			//);
		}
    }
}
