using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.UtilitsNamespace;
using cqa_medical.UtilitsNamespace.Parsers;

namespace cqa_medical
{
	internal class Program
	{
		public const string FilesDirectory = "../../Files/";
		public const string StatisticsDirectory = "../../StatOutput/";

		public const string MailUsersDirectory = StatisticsDirectory + "userInfos2/";

		public const string QuestionsNoTyposFileName = FilesDirectory + "qst_25.NoTypos.csv";
		public const string AnswersNoTyposFileName = FilesDirectory + "ans_25.NoTypos.csv";
		public const string QuestionsFileName = FilesDirectory + "qst_25.csv";
		public const string AnswersFileName = FilesDirectory + "ans_25.csv";

		public const string DeseasesFileName = FilesDirectory + "Deseases.txt";
		public const string LazarevaManualFileName = FilesDirectory + "Лазарева - Справочник фельдшера.txt";
		public const string BodyPartsFileName = FilesDirectory + "BodyParts.txt";
		public const string MedicamentsFileName = FilesDirectory + "Grls.txt";

		public const string FemaleNamesFileName = FilesDirectory + "Female_names.txt";
		public const string MaleNamesFileName = FilesDirectory + "Male_names.txt";

		public const string DeseasesIndexFileName = FilesDirectory + "DeseasesIndex.txt";
		public const string MedicamentsIndexFileName = FilesDirectory + "MedicamentsIndex.txt";
		public const string SymptomsIndexFileName = FilesDirectory + "SymptomsIndex.txt";
		public const string RussianCitiesFileName = "../../UtilitsNamespace/cities.txt";

		public const int TopicsCount = 100;
		public const string TopicsWordsFileName = FilesDirectory + "100_topics_health.twords";
		public const string TopicsFileName = FilesDirectory + "100_topics_health.theta";
		public const string DocIdsFileName = FilesDirectory + "GibbsDocIds_health.txt";
		
		public const string TestQuestionsFileName = "../../../../FilesToCommit/QuestionsTest.csv";
		public const string TestAnswersFileName = "../../../../FilesToCommit/AnswersTest.csv";


		private static readonly Lazy<Vocabulary> DefaultVocabularyLazy =
			new Lazy<Vocabulary>(() => new Vocabulary(QuestionsFileName, AnswersFileName));
		public static Vocabulary DefaultVocabulary
		{
			get { return DefaultVocabularyLazy.Value; }
		}
		
		private static readonly Lazy<Dictionary<string, MailUser>> DefaultMailUsersLazy =
		new Lazy<Dictionary<string, MailUser>>(() =>
		{
			var parser = new MailUserPageParser(MailUsersDirectory);
			return parser.ParsePages().ToDictionary(u => u.Email, u => u);
		});

		public static Dictionary<string, MailUser> DefaultMailUsers
		{
			get { return DefaultMailUsersLazy.Value; }
		}

		private static readonly Lazy<MyStemmer> DefaultMyStemmerLazy =
			new Lazy<MyStemmer>(() => new MyStemmer(QuestionsFileName, AnswersFileName));
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

		private static readonly Lazy<QuestionList> DefaultNotStemmedQuestionListLazy =
			new Lazy<QuestionList>(() => new QuestionList(QuestionsFileName, AnswersFileName));
		public static QuestionList DefaultNotStemmedQuestionList
		{
			get { return DefaultNotStemmedQuestionListLazy.Value; }
		}

		private static readonly Lazy<QuestionList> TestDefaultQuestionListLazy =
			new Lazy<QuestionList>(() => new QuestionList(TestQuestionsFileName, TestAnswersFileName, DefaultMyStemmer));
		public static QuestionList TestDefaultQuestionList
		{
			get { return TestDefaultQuestionListLazy.Value; }
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
				var q = DefaultQuestionList;
				Console.WriteLine(q.GetAllQuestions().First());
			}

			[Test, Explicit]
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
			[Test]
			public static void CheckQuestionListTypos()
			{
//				if (DataActualityChecker.IsFileActual(QuestionsNoTyposFileName, new[] { QuestionsFileName }) &&
//					  DataActualityChecker.IsFileActual(AnswersNoTyposFileName, new[] { AnswersFileName })) return;
				var ql = DefaultNotStemmedQuestionList;
				SpellChecker.SpellChecker.ModifyTyposCorpus(ql);
				var actual = ql.GetAllQuestions().First(q => q.Id == 68484951);
				Assert.That(actual.Text, Is.EqualTo("одна знакомая плюсной пользуется ветеринарными свечами для себя для профилактики женских заболеваний знаете название свечей и не вредно ли это"));

			}
			[Test, Explicit]
			public static void StplitToWords()
			{
				var ql = DefaultNotStemmedQuestionList;

				new Action(
					()=>
						{
							foreach (var question in ql.GetAllQuestions())
							{
								question.Text = String.Join(" ", question.Text.SplitInWordsAndStripHTML());
								question.Title = String.Join(" ", question.Title.SplitInWordsAndStripHTML());
							}
						}).DetectTime("Questions Modified");

				new Action(
					()=>
						{
							foreach (var answer in ql.GetAllAnswers())
							{
								answer.Text = String.Join(" ", answer.Text.SplitInWordsAndStripHTML());
							}
						}).DetectTime("Answers Modified");

				File.WriteAllLines(AnswersFileName + "ol.txt", ql.GetAllAnswers().Select(Answer.FormatStringWrite));
				File.WriteAllLines(QuestionsFileName + "ol.txt", ql.GetAllQuestions().Select(Question.FormatStringWrite));
			}
		}
		
	}
}
