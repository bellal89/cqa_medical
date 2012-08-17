using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Iveonik.Stemmers;
using NUnit.Framework;
using cqa_medical.BodyAnalisys;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.DataInput.Stemmers.AOTLemmatizer;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.UtilitsNamespace;


namespace cqa_medical.Statistics
{
	public class StatisticsAttribute : Attribute
	{
	}

	internal class Statistics
	{
		private readonly QuestionList questionList;
		private readonly Question[] questions;
		private readonly Answer[] answers;
		public static readonly DateTime FirstDate = new DateTime(2011, 9, 26, 1, 2, 3);

		public Statistics(QuestionList questionList)
		{
			this.questionList = questionList;
			questions = questionList.GetAllQuestions().ToArray();
			answers = questionList.GetAllAnswers().ToArray();
		}
		
	

		private SortedDictionary<T, int> GetDistribution<T>(IEnumerable<T> data)
		{
			return new DistributionCreator<T>(data).GetData();
		}

		public SortedDictionary<string, double> WordQuotientDistributionInWeeks(IEnumerable<string> expectedWords)
		{
			var enumerator = WordIntensityDistributionInWeeks(expectedWords);
			var denumerator = GetDistribution(questions
			                                  	.Where(a => a.DateAdded >= FirstDate)
			                                  	.Select(q => q.DateAdded.AddDays(-(int)q.DateAdded.DayOfWeek).ToShortDateString()));
												//.Select(q => GetWeek(q.DateAdded).ToShortDateString()));
			return Utilits.DistributionQuotient(enumerator, denumerator);
		}

		#region SimpleStatistics
		[Statistics]
		public SortedDictionary<int, int> AnswerLengthDistibution()
		{
			return GetDistribution(answers.Select(t => t.Text.Length));
		}

		[Statistics]
		public SortedDictionary<int, int> AnswersAmountDistibution()
		{
			return GetDistribution(questions.Select(t => t.GetAnswers().Count));
		}

		[Statistics]
		public SortedDictionary<int, int> AnswerSpeedDistibution()
		{
			return
				GetDistribution(
					answers.Select(t => (int) Math.Floor((t.DateAdded - questionList.GetQuestion(t.QuestionId).DateAdded).TotalMinutes)));
		}

		[Statistics]
		public SortedDictionary<int, int> QuestionLengthDistibution()
		{
			return GetDistribution(questions.Select(t => t.WholeText.Length));
		}

		[Statistics]
		public SortedDictionary<string, int> QuestionActivityInDaysDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.ToShortDateString()));
		}

		[Statistics]
		public SortedDictionary<string, int> AnswerActivityInDaysDistibution()
		{
			return GetDistribution(answers.Select(t => t.DateAdded.ToShortDateString()));
		}

		[Statistics]
		public SortedDictionary<string, int> QuestionActivityInDaysByWeekDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.DayOfWeek.ToString()));
		}

		[Statistics]
		public SortedDictionary<int, int> QuestionActivityInHoursByDayDistibution()
		{
			return GetDistribution(questions.Select(t => t.DateAdded.Hour));
		}

		[Statistics]
		public SortedDictionary<string, int> UserActivityInQuestionsDistibution()
		{
			return GetDistribution(questions.Select(t => t.AuthorEmail));
		}

		[Statistics]
		public SortedDictionary<string, int> UserActivityInAnswersDistibution()
		{
			return GetDistribution(answers.Select(t => t.AuthorEmail));
		}

		[Statistics]
		public SortedDictionary<string, int> UserActivityInMessagesDistibution()
		{
			var statisticGenerator = new DistributionCreator<string>(questions.Select(t => t.AuthorEmail));
			statisticGenerator.AddData(answers.Select(t => t.AuthorEmail));
			return statisticGenerator.GetData();
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryQuestionsDistribution()
		{
			return GetDistribution(questions.Select(q => q.Category));
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryAnswersDistribution()
		{
			return GetDistribution(answers.Select(a => questionList.GetQuestion(a.QuestionId).Category));
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryUsersDistribution()
		{
			var categories = new HashSet<Tuple<string, string>>();
			foreach (var answer in answers)
			{
				var question = questionList.GetQuestion(answer.QuestionId);
				categories.Add(new Tuple<string, string>(question.Category, question.AuthorEmail));
				categories.Add(new Tuple<string, string>(question.Category, answer.AuthorEmail));
			}
			return GetDistribution(categories.Select(cat => cat.Item1));
		}

		[Statistics]
		public SortedDictionary<int, int> AnswerLengthInWordsDistribution()
		{
			return GetDistribution(answers
			                       	.Select(a => a.Text.SplitInWordsAndStripHTML().ToArray().Length));
		}

		[Statistics]
		public SortedDictionary<int, int> QuestionLengthInWordsDistribution()
		{
			return GetDistribution(questions
			                       	.Select(a => a.WholeText.SplitInWordsAndStripHTML().ToArray().Length));
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryUserQuestionsDistribution()
		{
			return GetDistribution(questions.Select(q => new Tuple<string, string>(q.Category, q.AuthorEmail))
			                       	.Distinct()
			                       	.Select(item => item.Item1));
		}

		[Statistics]
		public SortedDictionary<string, int> CategoryUserAnswersDistribution()
		{
			return
				GetDistribution(answers.Select(
					a => new Tuple<string, string>(questionList.GetQuestion(a.QuestionId).Category, a.AuthorEmail))
				                	.Distinct()
				                	.Select(item => item.Item1));
		}

		/// <summary>
		/// This distribution is statistics of another type
		/// </summary>
		/// <returns>{user => (questionsAmount, answersAmount)}</returns>
		[Statistics]
		public Dictionary<string, Tuple<int, int>> QuestionsAnswersUserDistribution()
		{
			SortedDictionary<string, int> questionsActivity = UserActivityInQuestionsDistibution();
			SortedDictionary<string, int> answersActivity = UserActivityInAnswersDistibution();
			var userNames = questionsActivity.Keys.Union(answersActivity.Keys);
			return userNames.ToDictionary(
				userName => userName,
				userName => Tuple.Create(questionsActivity.GetOrDefault(userName, 0), answersActivity.GetOrDefault(userName, 0)));
		}

		[Statistics]
		public SortedDictionary<int, int> QuestionsAmountPerUserDistribution()
		{
			return GetDistribution(questions.GroupBy(q => q.AuthorEmail, (email, qs) => qs.Count()));
		}

		[Statistics]
		public SortedDictionary<int, int> AnswersAmountPerUserDistribution()
		{
			return GetDistribution(answers.GroupBy(a => a.AuthorEmail, (email, qs) => qs.Count()));
		}
		#endregion

		public SortedDictionary<string, int> WordIntensityDistributionInWeeks(IEnumerable<string> expectedWords)
		{
			return GetDistribution(questions
			                       	.Where(a => a.DateAdded >= FirstDate)
			                       	.Where(q => OneOfWordsInsideTheText(q.WholeText + String.Join(" ", q.GetAnswers().Select(a => a.Text)), expectedWords))
									.Select(q => Utilits.GetWeek(q.DateAdded).ToShortDateString()));
		}

		public SortedDictionary<DateTime, int> WordIntensityDistributionInDays(IEnumerable<string> expectedWords)
		{
			var keyWords = expectedWords.Select(s => s.ToLower());
			return GetDistribution(questions.Where(q => OneOfWordsInsideTheText(q.WholeText, keyWords)).Select(q => q.DateAdded.Date));
		}

		private static bool OneOfWordsInsideTheText(string text, IEnumerable<string> keyWords)
		{
			return keyWords.Select(w => w.ToLower()).Any(text.SplitInWordsAndStripHTML().Contains);
		}

		public SortedDictionary<string, int> WordFrequency(IStemmer stemmer)
		{
			var statisticGenerator =
				new DistributionCreator<string>(
					questions.SelectMany(t => t.WholeText.SplitInWordsAndStripHTML()).Select(stemmer.Stem));
			statisticGenerator.AddData(answers.SelectMany(t => t.Text.SplitInWordsAndStripHTML()).Select(stemmer.Stem));
			return statisticGenerator.GetData();
		}

		public SortedDictionary<string, double> SymptomIntensityDistributionInDays(InvertedIndexUnit symptom)
		{
			var numerator = GetDistribution(symptom.Ids.Select(id => questionList.GetQuestion(id).DateAdded.AddDays(-(int)questionList.GetQuestion(id).DateAdded.DayOfWeek).ToShortDateString()));
			var denominator = GetDistribution(questions.Select(q => q.DateAdded.AddDays(-(int)q.DateAdded.DayOfWeek).ToShortDateString()));
			return UtilitsNamespace.Utilits.DistributionQuotient(numerator, denominator);
		}

		public static double GetAverage(IDictionary<int, int> distribution)
		{
			return distribution.Keys.Select(k => k*distribution[k]).Sum()/(double) distribution.Values.Sum();
		}

		public SortedDictionary<DateTime, int> WordIntensity(IEnumerable<string> expectedWords)
		{
			return GetDistribution(questions
									.Where(a => a.DateAdded >= FirstDate)
									.Where(q => OneOfWordsInsideTheText(q.WholeText + String.Join(" ", q.GetAnswers().Select(a => a.Text)), expectedWords))
									.Select(q => Utilits.GetWeek(q.DateAdded)));
		}

	}


	[TestFixture]
	internal class GetDistributions
	{
		private static Statistics statistics;

		[TestFixtureSetUp]
		public void DistributionInit()
		{
			var ql = Program.DefaultQuestionList;
			statistics = new Statistics(ql);
		}

		[Test, Explicit]
		public void AverageThread()
		{
			Console.WriteLine("AverageThreadLength: {0}", Statistics.GetAverage(statistics.AnswersAmountDistibution()));
			Console.WriteLine("AverageAnswerLength in symbols: {0}", Statistics.GetAverage(statistics.AnswerLengthDistibution()));
			Console.WriteLine("AverageAnswerLength in words: {0}", Statistics.GetAverage(statistics.AnswerLengthInWordsDistribution()));
			Console.WriteLine("AverageQuestionLength in symbols: {0}", Statistics.GetAverage(statistics.QuestionLengthDistibution()));
			Console.WriteLine("AverageQuesitonLength in words: {0}", Statistics.GetAverage(statistics.QuestionLengthInWordsDistribution()));
		}
		[Test, Explicit]
		public void AverageAnswerLength()
		{
			
		}
		[Test, Explicit]
		public void SymptomsOverTimeDistribution()
		{
			const int numberOfSyptoms = 10;
			var symptoms = Symptoms.GetDefault().OrderByDescending(s => s.Ids.Count).Take(numberOfSyptoms);
			foreach (var symptom in symptoms)
			{
				var data = statistics.SymptomIntensityDistributionInDays(symptom);
				File.WriteAllText(Program.StatisticsDirectory + "Symptom distributions/Symptom distrib - " + symptom.Word + ".txt",
								  String.Join("\n", data.Select(item => item.Key + "\t" + item.Value)));
			}
		}

		[Test, Explicit]
		public void CommonStatistics()
		{
			IEnumerable<MethodInfo> infos = statistics
				.GetType()
				.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(m => m.GetCustomAttributes(typeof (StatisticsAttribute), true).Any()).ToList();

			var rawMethod = typeof (UtilitsNamespace.Utilits).GetMethod("ToStringNormal");
			foreach (var info in infos)
			{
				Console.WriteLine("calculating " + info.Name);
				var data = info.Invoke(statistics, new object[0]);
				var genericMethod = rawMethod.MakeGenericMethod(data.GetType().GetGenericArguments());
				var outString = genericMethod.Invoke(null, new[] {data});
				File.WriteAllText(Program.StatisticsDirectory + info.Name + ".txt", (string) outString);
			}
		}

		[Test, Explicit]
		public void WordsContainingDistributionInDays()
		{
			var words = new []{"температура"};

			Console.WriteLine("calculating WordQuotientDistributionInWeeks, words: " + String.Join(", ", words));
			var data = statistics.WordIntensityDistributionInDays(words).ToDictionary(item => item.Key, item => item.Value);
			var formattedData = data.SumUpToDays().ToStringComparable();
//			var formattedData = data.ToStringNormal();
			File.WriteAllText(
				Program.StatisticsDirectory + "WordIntensityDistributionInDays_" + String.Join("_", words) + ".txt", formattedData);

			Console.WriteLine(new OctavePlot("2.png", data.Keys.ToArray(), data.Values.Select(k => (double)k).ToArray()).DrawPlot());
		}

		[Test, Explicit, TestCaseSource("DivideCases")]
		public void WordQuotientDistributionInWeeks(string[] expectedWords)
		{
			Console.WriteLine("calculating WordQuotientDistributionInWeeks, words: " + String.Join(", ", expectedWords));
			var data = statistics.WordQuotientDistributionInWeeks(expectedWords.Select(Program.DefaultMyStemmer.Stem)).ToStringNormal();
			File.WriteAllText(
				Program.StatisticsDirectory + "WordQuotientDistributionInWeeks_" + String.Join("_", expectedWords) + ".txt", data);
		}

		[Test, Explicit, TestCaseSource("DivideCases")]
		public void WordIntensityDistributionInWeeks(string[] expectedWords)
		{
			var cul = CultureInfo.InvariantCulture;
			Console.WriteLine("calculating WordIntensityDistributionInWeeks, words: " + String.Join(", ", expectedWords));
			var data = statistics.WordIntensity(expectedWords);
//			var formattedData = data.ToStringNormal();

			var foramttedData = string.Join(Environment.NewLine, data.SumUpToDays().ToStringComparable());
			File.WriteAllText(
				Program.StatisticsDirectory + "WordIntensityDistributionInWeeks_" + String.Join("_", expectedWords) + ".txt", foramttedData);
		}
		public static object[] DivideCases = new object[]
		                                      	{
		                                      		new object[] {new[] {"грипп", "ОРВИ"}}
		                                      	};

		[Test, Explicit]
		public void WordFrequency()
		{
			var stemmerCases = new[]
			                   	{
									new CaseT(new MyStemmer(new Vocabulary(Program.QuestionsFileName, Program.AnswersFileName)), new TimeSpan(0, 4, 1)),
			                   		new CaseT(new RussianStemmer(), new TimeSpan(0, 4, 20)),
			                   		new CaseT(new AOTLemmatizer(), new TimeSpan(6, 45, 0))
			                   	};
			foreach (var q in stemmerCases)
			{
				Console.WriteLine("Считает WordFrequency_" + q.Stemmer);
				Console.WriteLine("Начало     " + DateTime.Now);
				Console.WriteLine("Завершить  " + DateTime.Now.AddMinutes( q.TimeToAdd.TotalMinutes));
				var data = statistics.WordFrequency(q.Stemmer).ToStringInverted();
				File.WriteAllText(
					Program.StatisticsDirectory + "WordFrequency_" + q.Stemmer + ".txt", data);
				Console.WriteLine("Свершилось " + DateTime.Now);
			}
		}

		[Test, Explicit]
		public void StoreTemperatureAndPressure()
		{
			
		}


	}
	struct CaseT
	{
		public IStemmer Stemmer;
		public TimeSpan TimeToAdd;

		public CaseT(IStemmer stemmer, TimeSpan timeToAdd)
		{
			Stemmer = stemmer;
			TimeToAdd = timeToAdd;
		}
	}
}
