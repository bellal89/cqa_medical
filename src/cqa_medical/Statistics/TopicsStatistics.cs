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
		public TopicsStatistics(QuestionList questionList) : base(questionList) {}

		public List<double>[] ReadTopics(string topicsFile, int nTopics)
		{
			var docs = new List<double>[nTopics];
			for (int i = 0; i < nTopics; i++)
			{
				docs[i] = new List<double>();
			}
			var lines = File.ReadLines(topicsFile);
			foreach (var line in lines)
			{
				var doubles = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(
					el => double.Parse(el, CultureInfo.InvariantCulture)).ToArray();
				for (var i = 0; i < doubles.Length; i++)
				{
					docs[i].Add(doubles[i]);
				}
			}
			return docs;
		}

		public void GetTopicDocuments(int topicNumber, string topicsFile, string docIdsFile, string fileToSave)
		{
			var ids = File.ReadAllLines(docIdsFile).Select(long.Parse).ToArray();
			var topics = ReadTopics(topicsFile, 100);

			var docs = topics[topicNumber].Select((t, i) => Tuple.Create(i, t)).OrderByDescending(d => d.Item2).ToArray();

			Console.WriteLine(docs.Length);
			Console.WriteLine("---");
			File.WriteAllLines(topicNumber + "_topic_docs.txt", docs.Select((d, i) => i + "\t" + ids[d.Item1] + "\t" + d.Item2));
		}

		public Dictionary<string, double> AverageTopicProbabilityDistributionInDays(int topicNumber, string docIdsFile, string topicsFile)
		{
			var ids = File.ReadAllLines(docIdsFile).Select(long.Parse).ToArray();
			var topic =
				File.ReadAllLines(topicsFile)
					.Select(
						line =>
						double.Parse(
							line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[topicNumber],
							CultureInfo.InvariantCulture))
					.ToArray();

			//			Assert.AreEqual(ids.Length, topic.Count());

			return ids.Zip(topic, Tuple.Create)
					  .GroupBy(doc => questionList.GetQuestion(doc.Item1).DateAdded.ToShortDateString(),
							   (key, docs) => Tuple.Create(key, docs.Average(doc => doc.Item2)))
					  .ToDictionary(item => item.Item1, item => item.Item2);
		}

		public SortedDictionary<string, double> MaxTopicProbabilityDocsDistributionInDays(int topicNumber, string docIdsFile, string topicsFile)
		{
			var ids = File.ReadAllLines(docIdsFile).Select(long.Parse).ToArray();
			var docs = ReadTopicFrom(topicNumber, topicsFile, (doc, nTopic) => Math.Abs(doc[nTopic] - doc.Max()) < 0.001);

			var numerator = GetDistribution(docs.Select(d => questionList.GetQuestion(ids[d]).DateAdded.ToShortDateString()));
			var denominator = GetDistribution(questions.Select(q => q.DateAdded.ToShortDateString()));
			return Utilits.DistributionQuotient(numerator, denominator);
		}

		public SortedDictionary<string, double> ThresholdTopicProbabilityDocsDistributionInDays(List<double>[] topicsTransposed, int topicNumber, double threshold, string docIdsFile, IDictionary<string, MailUser> mailUsers, string userGeo)
		{
			var ids = File.ReadAllLines(docIdsFile).Select(long.Parse).ToArray();
			//var docs = ReadTopicFrom(topicNumber, topicsFile, (doc, nTopic) => doc[nTopic] > threshold);
			var docs = topicsTransposed[topicNumber]
						.Select((t, i) => Tuple.Create(i, t))
						.Where(tpl => tpl.Item2 > threshold)
						.Select(tpl => tpl.Item1);

			MailUser user;
			var numerator = GetDistribution(docs.Where(d => mailUsers.TryGetValue(questionList.GetQuestion(ids[d]).AuthorEmail, out user) && user.Geo == userGeo)
												.Select(d => questionList.GetQuestion(ids[d]).DateAdded.ToShortDateString()));
			var denominator = GetDistribution(questions.Where(q => mailUsers.TryGetValue(q.AuthorEmail, out user) && user.Geo == userGeo).Select(q => q.DateAdded.ToShortDateString()));
			return Utilits.DistributionQuotient(numerator, denominator);
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
			topicsStatistics = new TopicsStatistics(q1);

			var parser = new MailUserPageParser(Program.MailUsersDirectory);
			mailUsers = parser.ParseUsers().ToDictionary(u => u.Email, u => u);

			Console.WriteLine("Preparations have been done");
		}

		[Test]
		[TestCase(2)]
		public void TestTopicDocs(int topicNumber)
		{
			topicsStatistics.GetTopicDocuments(topicNumber,
			                                   @"c:\Users\beloborodov\Documents\GibbsLDA\GibbsLDA++-0.2\CQA_LDA\100_topics_health.theta",
			                                   "GibbsDocIdsCat.txt", topicNumber + ".txt");
		}

		[Test, Explicit]
		// 196 - Flu topic
		// 197 - Mindless, shisophrenia
		// 159 - Weight
		[TestCase(159)]
		public void AverageTopicsOverDaysDistribution(int topicNumber)
		{
			var fluTopicDistrib = topicsStatistics.AverageTopicProbabilityDistributionInDays(topicNumber,
														  Program.GibbsDocIdsFileName,
														  Program.ThetaFileName);
			var fileToSave = Program.StatisticsDirectory + "Topic_distributions/" + topicNumber + "_sm";
			File.WriteAllText(fileToSave + ".txt",
				String.Join("\n", fluTopicDistrib.Select(t => t.Key + "\t" + t.Value)));

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

			var fluTopicDistrib = topicsStatistics.MaxTopicProbabilityDocsDistributionInDays(topicNumber,
														  Program.GibbsDocIdsFileName,
														  Program.ThetaFileName);
			File.WriteAllText(Program.StatisticsDirectory + "Topic_distributions/" + topicNumber + "_max.txt",
				String.Join("\n", fluTopicDistrib.Select(t => t.Key + "\t" + t.Value)));
		}

		[Test, Explicit]
		[TestCase(5)]
		public void ThresholdTopicDistribution(int topicNumber)
		{
			const int allTopicsNumber = 360;

			var topics = topicsStatistics.ReadTopics(Program.ThetaFileName, allTopicsNumber);
			var distr = topicsStatistics.ThresholdTopicProbabilityDocsDistributionInDays(topics,
																				   topicNumber,
																				   0.1,
																				   Program.GibbsDocIdsFileName,
																				   mailUsers,
																				   "москва"
																					);

			File.WriteAllText(Program.StatisticsDirectory + "Topic_distributions/" + topicNumber + "_moscow.txt", String.Join("\n", distr.Select(it => it.Key + '\t' + it.Value)));
		}

		[Test, Explicit]
		public void SearchInterestingThresholdDistribs()
		{
			const int allTopicsNumber = 360;

			var topics = topicsStatistics.ReadTopics(Program.ThetaFileName, allTopicsNumber);
			var interesting = new List<Tuple<int, double>>();

			for (int i = 0; i < allTopicsNumber; i++)
			{
				var fluTopicDistrib = topicsStatistics.ThresholdTopicProbabilityDocsDistributionInDays(topics, i,
															  0.1,
															  Program.GibbsDocIdsFileName, mailUsers, "москва");

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
