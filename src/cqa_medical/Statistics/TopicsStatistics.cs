using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Statistics
{
	class TopicsStatistics : BaseStatistics
	{
		private readonly long[] ids;
		private readonly List<double>[] topics;

		public int GetTopicsCount()
		{
			return topics.Length;
		}

		public TopicsStatistics(QuestionList questionList, string docIdsFile, string topicsFile) : base(questionList)
		{
			ids = File.ReadAllLines(docIdsFile).Select(long.Parse).ToArray();
			topics = ReadTopicsFrom(topicsFile, 100);
		}

		private static IEnumerable<int> ReadTopicFrom(int topicNumber, string topicsFile, Func<double[], int, bool> docHandler)
		{
			var docs = new List<int>();
			using (var f = new StreamReader(topicsFile))
			{
				var i = 0;
				while (!f.EndOfStream)
				{
					var line = f.ReadLine();
					if (line == null) break;
					var doc = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(
						el => double.Parse(el, CultureInfo.InvariantCulture)).ToArray();
					if (docHandler(doc, topicNumber))
					{
						docs.Add(i);
					}
					i++;
				}
			}
			return docs;
		}

		public List<double>[] ReadTopicsFrom(string topicsFile, int nTopics)
		{
			var topicList = new List<double>[nTopics];
			for (var i = 0; i < nTopics; i++)
			{
				topicList[i] = new List<double>();
			}
			var lines = File.ReadLines(topicsFile);
			foreach (var line in lines)
			{
				var doubles = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(
					el => double.Parse(el, CultureInfo.InvariantCulture)).ToArray();
				for (var i = 0; i < doubles.Length; i++)
				{
					topicList[i].Add(doubles[i]);
				}
			}
			return topicList;
		}

		public IEnumerable<Tuple<long, double>> GetTopicDocuments(int topicNumber)
		{
			return topics[topicNumber].Select((t, i) => Tuple.Create(ids[i], t)).OrderByDescending(d => d.Item2);
		}

		public Dictionary<string, double> AverageTopicProbabilityDistributionInDays(int topicNumber, string topicsFile)
		{
			var topic = topics[topicNumber].Select((t, i) => Tuple.Create(ids[i], t));

			return topic.GroupBy(doc => QuestionList.GetQuestion(doc.Item1).DateAdded.ToShortDateString(),
							   (key, docs) => Tuple.Create(key, docs.Average(doc => doc.Item2)))
						.ToDictionary(item => item.Item1, item => item.Item2);
		}

		public SortedDictionary<string, double> MaxTopicProbabilityDocsDistributionInDays(int topicNumber, string topicsFile)
		{
			var docs = GetMaxTopicDocs(topicNumber, topicsFile);

			var numerator = GetDistribution(docs.Select(d => QuestionList.GetQuestion(ids[d]).DateAdded.ToShortDateString()));
			var denominator = GetDistribution(Questions.Select(q => q.DateAdded.ToShortDateString()));
			return Utilits.DistributionQuotient(numerator, denominator);
		}

		private static IEnumerable<int> GetMaxTopicDocs(int topicNumber, string topicsFile)
		{
			var docs = ReadTopicFrom(topicNumber, topicsFile, (doc, nTopic) => Math.Abs(doc[nTopic] - doc.Max()) < 0.001);
			return docs;
		}

		public SortedDictionary<string, double> ThresholdTopicProbabilityDocsDistributionInDays(int topicNumber, double threshold, IDictionary<string, MailUser> mailUsers, string userGeo)
		{
			var docs = topics[topicNumber]
						.Select((t, i) => Tuple.Create(i, t))
						.Where(tpl => tpl.Item2 > threshold)
						.Select(tpl => tpl.Item1);

			MailUser user;
			var numerator = GetDistribution(docs.Where(d => mailUsers.TryGetValue(QuestionList.GetQuestion(ids[d]).AuthorEmail, out user) && user.Geo == userGeo)
												.Select(d => QuestionList.GetQuestion(ids[d]).DateAdded.ToShortDateString()));
			var denominator = GetDistribution(Questions.Where(q => mailUsers.TryGetValue(q.AuthorEmail, out user) && user.Geo == userGeo).Select(q => q.DateAdded.ToShortDateString()));
			return Utilits.DistributionQuotient(numerator, denominator);
		}
	}

	[TestFixture]
	internal class TopicsDistributions
	{
		private TopicsStatistics topicsStatistics;
		private Dictionary<string, MailUser> mailUsers;

		[SetUp]
		public void DistributionInit()
		{
			var q1 = Program.DefaultQuestionList;
			topicsStatistics = new TopicsStatistics(q1, Program.DocIdsFileName, Program.TopicsFileName);

			var parser = new MailUserPageParser(Program.MailUsersDirectory);
			mailUsers = parser.ParseUsers().ToDictionary(u => u.Email, u => u);

			Console.WriteLine("Preparations have been done");
		}

		[Test]
		[TestCase(2)]
		public void TestTopicDocs(int topicNumber)
		{
			var docs = topicsStatistics.GetTopicDocuments(topicNumber).ToList();
			Console.WriteLine("Docs count = " + docs.Count);
			Console.WriteLine(String.Join("\n", docs.Select(d => d.Item1 + "\t" + d.Item2)));
		}

		[Test, Explicit]
		// 196 - Flu topic
		// 197 - Mindless, shisophrenia
		// 159 - Weight
		[TestCase(159)]
		public void AverageTopicsOverDaysDistribution(int topicNumber)
		{
			var fluTopicDistrib = topicsStatistics.AverageTopicProbabilityDistributionInDays(topicNumber, Program.TopicsFileName);
			var fileToSave = Program.StatisticsDirectory + "Topic_distributions/" + topicNumber + "_sm";
			File.WriteAllText(fileToSave + ".txt", String.Join("\n", fluTopicDistrib.Select(t => t.Key + "\t" + t.Value)));

			var sortedDistrib1 =
				new SortedDictionary<DateTime, double>(fluTopicDistrib.ToDictionary(f => DateTime.Parse(f.Key), f => f.Value));
			var sortedDistrib = sortedDistrib1.SumUpToWeeks();

			new OctavePlot(fileToSave + ".png", sortedDistrib.Keys.ToArray(), sortedDistrib.Values.ToArray())
			{
				Title = "Распределение топика " + topicNumber + " по дням"
			}.DrawPlot();
		}

		[Test, Explicit]
		[TestCase(196)]
		//		[TestCase(197)]
		public void MaxTopicChanceDocsOverDaysDistribution(int topicNumber)
		{

			var fluTopicDistrib = 
				topicsStatistics.MaxTopicProbabilityDocsDistributionInDays(topicNumber, Program.TopicsFileName);

			File.WriteAllText(Program.StatisticsDirectory + "Topic_distributions/" + topicNumber + "_max.txt",
			                  String.Join("\n", fluTopicDistrib.Select(t => t.Key + "\t" + t.Value)));
		}

		[Test, Explicit]
		[TestCase(5)]
		public void ThresholdTopicDistribution(int topicNumber)
		{
			var distr = topicsStatistics
				.ThresholdTopicProbabilityDocsDistributionInDays(topicNumber, 0.1, mailUsers, "москва");

			File.WriteAllText(Program.StatisticsDirectory + "Topic_distributions/" + topicNumber + "_moscow.txt",
			                  String.Join("\n", distr.Select(it => it.Key + '\t' + it.Value)));
		}

		[Test, Explicit]
		public void SearchInterestingThresholdDistribs()
		{
			var interesting = new List<Tuple<int, double>>();

			for (var i = 0; i < topicsStatistics.GetTopicsCount(); i++)
			{
				var fluTopicDistrib = 
					topicsStatistics.ThresholdTopicProbabilityDocsDistributionInDays(i, 0.1, mailUsers, "москва");

				if (fluTopicDistrib.Any())
					interesting.Add(Tuple.Create(i, fluTopicDistrib.Max(it => it.Value) - fluTopicDistrib.Min(it => it.Value)));
				else
					Console.Write("Distrib is empty: ");
				Console.WriteLine(i);
			}
			Console.WriteLine(String.Join("\n", interesting.OrderByDescending(t => t.Item2).Select(t => t.Item1 + "\t" + t.Item2)));
		}
	}
}
