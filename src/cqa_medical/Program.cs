using System;
using System.IO;
using System.Linq;
using System.Text;
using Iveonik.Stemmers;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.BodyAnalisys;
using cqa_medical.Utilits;

namespace cqa_medical
{
    class Program
    {
    	public const string StatisticsDirectory = "../../StatOutput/";
		public const string QuestionsFileName = "../../Files/qst_25.csv";
		public const string AnswersFileName = "../../Files/ans_25.csv";
		public const string DeseasesFileName = "../../Files/Deseases.txt";
		public const string BodyPartsFileName = "../../Files/BodyParts.txt";
		public const string MedicamentsFileName = "../../Files/Grls.txt";

		public const string TestQuestionsFileName = "../../../../files/QuestionsTest.csv";
		public const string TestAnswersFileName = "../../../../files/AnswersTest.csv";

		public static QuestionList ParseAndStem()
		{
			var questionList = Parse(QuestionsFileName, AnswersFileName);
			return questionList.StemIt(new RussianStemmer());
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

		[TestFixture]
		public class ProgramTest
		{
			[Test]
			public void TestId()
			{
				var ql = Parse(QuestionsFileName, AnswersFileName);
				var hasIdenticId = false;
				foreach (var question in ql.GetAllQuestions())
				{

					foreach (var answer in ql.GetAllAnswers())
					{
						hasIdenticId = true;
						if (answer.Id == question.Id)
							Console.WriteLine("BAD ID!!!!!!!!! " + answer.Id);
					}
					//Console.WriteLine(question.Id);
				}
				Assert.AreEqual(true, hasIdenticId);
			}
		}
		

    	

    	
    }
}
