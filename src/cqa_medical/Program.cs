using System;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.Utilits;

namespace cqa_medical
{
	internal class Program
	{
		public const string FilesDirectory = "../../Files/";
		public const string StatisticsDirectory = "../../StatOutput/";
		public const string QuestionsFileName = FilesDirectory + "qst_25.csv";
		public const string AnswersFileName = FilesDirectory + "ans_25.csv";
		public const string DeseasesFileName = FilesDirectory + "Deseases.txt";
		public const string NotDeseasesFileName = FilesDirectory + "notDeseases.txt";
		public const string BodyPartsFileName = FilesDirectory + "BodyParts.txt";
		public const string MedicamentsFileName = FilesDirectory + "Grls.txt";
		public const string DeseasesIndexFileName = FilesDirectory + "DeseasesIndex.txt";
		public const string MedicamentsIndexFileName = FilesDirectory + "MedicamentsIndex.txt";
		public const string SymptomsIndexFileName = FilesDirectory + "SymptomsIndex.txt";

		public const string TestQuestionsFileName = "../../../../files/QuestionsTest.csv";
		public const string TestAnswersFileName = "../../../../files/AnswersTest.csv";


		private static readonly Lazy<Vocabulary> DefaultVocabularyLazy =
			new Lazy<Vocabulary>(() => new Vocabulary(QuestionsFileName, AnswersFileName));
		public static Vocabulary DefaultVocabulary
		{
			get { return DefaultVocabularyLazy.Value; }
		}

		private static readonly Lazy<MyStemmer> DefaultMyStemmerLazy =
			new Lazy<MyStemmer>(() => new MyStemmer(DefaultVocabulary));
		public static MyStemmer DefaultMyStemmer
		{
			get { return DefaultMyStemmerLazy.Value; }
		}

		private static readonly Lazy<QuestionList> DefaultQuestionListLazy =
			new Lazy<QuestionList>(() => new QuestionList(QuestionsFileName, AnswersFileName, DefaultMyStemmer));
		public static QuestionList DefaultQuestionList
		{
			get { return DefaultQuestionListLazy.Value; }
		}

		private static readonly Lazy<QuestionList> TestDefaultQuestionListLazy =
			new Lazy<QuestionList>(() => new QuestionList(TestQuestionsFileName, TestAnswersFileName, DefaultMyStemmer));
		public static QuestionList TestDefaultQuestionList
		{
			get { return TestDefaultQuestionListLazy.Value; }
		}

		private static readonly Lazy<QuestionList> DefaultNotStemmedQuestionListLazy =
			new Lazy<QuestionList>(() => new QuestionList(QuestionsFileName, AnswersFileName));
		public static QuestionList DefaultNotStemmedQuestionList
		{
			get { return DefaultNotStemmedQuestionListLazy.Value; }
		}

		private static readonly Lazy<QuestionList> TestDefaultNotStemmedQuestionListLazy =
			new Lazy<QuestionList>(() => new QuestionList(TestQuestionsFileName, TestAnswersFileName));
		public static QuestionList TestDefaultNotStemmedQuestionList
		{
			get { return TestDefaultNotStemmedQuestionListLazy.Value; }
		}


		[TestFixture]
		public class ProgramTest
		{
			[Test]
			public void Getq()
			{
				var q = DefaultNotStemmedQuestionList;
			}

			[Test]
			public void TestId()
			{
				var ql = new QuestionList(QuestionsFileName, AnswersFileName);
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
