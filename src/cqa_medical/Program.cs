using System;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;

namespace cqa_medical
{
    class Program
    {
    	public const string StatisticsDirectory = "../../StatOutput/";
		public const string QuestionsFileName = "../../Files/qst_25.csv";
		public const string AnswersFileName = "../../Files/ans_25.csv";
		public const string DeseasesFileName = "../../Files/Deseases.txt";
		public const string NotDeseasesFileName = "../../Files/notDeseases.txt";
		public const string BodyPartsFileName = "../../Files/BodyParts.txt";
		public const string MedicamentsFileName = "../../Files/Grls.txt";
		public const string DeseasesIndexFileName = "../../Files/DeseasesIndex.txt";
		public const string MedicamentsIndexFileName = "../../Files/MedicamentsIndex.txt";
		public const string SymptomsIndexFileName = "../../Files/SymptomsIndex.txt";

		public const string TestQuestionsFileName = "../../../../files/QuestionsTest.csv";
		public const string TestAnswersFileName = "../../../../files/AnswersTest.csv";

		
		private static readonly Lazy<Vocabulary> DefaultVocabularyLazy = new Lazy<Vocabulary>(() => new Vocabulary(QuestionsFileName, AnswersFileName));
		public static Vocabulary DefaultVocabulary{get { return DefaultVocabularyLazy.Value; }}

		private static readonly Lazy<MyStemmer> DefaultMyStemmerLazy = new Lazy<MyStemmer>(() => new MyStemmer(DefaultVocabulary));
		public static MyStemmer DefaultMyStemmer { get { return DefaultMyStemmerLazy.Value; } }

    	private static readonly Lazy<QuestionList> DefaultQuestionListLazy = new Lazy<QuestionList>(ParseAndStem);
		public static QuestionList DefaultQuestionList { get { return DefaultQuestionListLazy.Value; }}

		private static readonly Lazy<QuestionList> TestDefaultQuestionListLazy = new Lazy<QuestionList>(ParseAndStemTest);
		public static QuestionList TestDefaultQuestionList { get { return TestDefaultQuestionListLazy.Value; } }
		

		public static QuestionList ParseAndStem()
		{
			var questionList = Parse(QuestionsFileName, AnswersFileName);
			return questionList.StemIt(DefaultMyStemmer);
		}
		public static QuestionList ParseAndStemTest()
		{
			var questionList = Parse(TestQuestionsFileName, TestAnswersFileName);
			return questionList.StemIt(DefaultMyStemmer);
		}

		public static QuestionList Parse(string questionsFileName, string answersFileName)
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
