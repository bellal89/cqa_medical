using System;
using cqa_medical.DataInput;

namespace cqa_medical
{
    class Program
    {
		
        static void Main(string[] args)
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
    }
}
