using System;
using cqa_medical.DataInput;
using cqa_medical.Statistics;
using cqa_medical.BodyAnalisys;

namespace cqa_medical
{
    class Program
    {
		private static void ParseAndStem()
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
			questionList.StemIt();
		}


        static void Main(string[] args)
        {
        	var q = BodyPart.GetBodyPartsFromFile(@"..\..\Files\BodyParts.txt");
        }

    	
    }
}
