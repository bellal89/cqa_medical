using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.LDA;
using cqa_medical.UtilitsNamespace;
using LinqLib.Sequence;

namespace cqa_medical.Statistics
{
	class TopicsStatistics : BaseStatistics
	{
		private readonly long[] questionIds;
		private readonly Dictionary<long, int> questionsToDocs;
		private readonly List<double>[] topics;

		public int GetTopicsCount()
		{
			return topics.Length;
		}

		public TopicsStatistics(QuestionList questionList, string docIdsFile = Program.DocIdsFileName, string topicsFile = Program.TopicsFileName, int allTopicsNumber = Program.TopicsCount) : base(questionList)
		{
			questionIds = File.ReadAllLines(docIdsFile).Select(Int64.Parse).ToArray();
			questionsToDocs = questionIds.Select((id, i) => new {docId = i, questionId = id})
										 .ToDictionary(doc => doc.questionId, doc => doc.docId);
			topics = ReadTopicsFrom(topicsFile, allTopicsNumber);
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
						el => Double.Parse(el, CultureInfo.InvariantCulture)).ToArray();
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
					el => Double.Parse(el, CultureInfo.InvariantCulture)).ToArray();
				for (var i = 0; i < doubles.Length; i++)
				{
					topicList[i].Add(doubles[i]);
				}
			}
			return topicList;
		}

		public IEnumerable<Tuple<long, double>> GetTopicDocuments(int topicNumber)
		{
			return topics[topicNumber].Select((t, i) => Tuple.Create(questionIds[i], t)).OrderByDescending(d => d.Item2);
		}

		public Dictionary<string, double> AverageTopicProbabilityDistributionInDays(int topicNumber, string topicsFile)
		{
			var topic = topics[topicNumber].Select((t, i) => Tuple.Create(questionIds[i], t));

			return topic.GroupBy(doc => QuestionList.GetQuestion(doc.Item1).DateAdded.ToShortDateString(),
							   (key, docs) => Tuple.Create(key, docs.Average(doc => doc.Item2)))
						.ToDictionary(item => item.Item1, item => item.Item2);
		}

		public SortedDictionary<string, double> MaxTopicProbabilityDocsDistributionInDays(int topicNumber, string topicsFile)
		{
			var docs = GetMaxTopicDocs(topicNumber, topicsFile);

			var numerator = GetDistribution(docs.Select(d => QuestionList.GetQuestion(questionIds[d]).DateAdded.ToShortDateString()));
			var denominator = GetDistribution(Questions.Select(q => q.DateAdded.ToShortDateString()));
			return Utilits.DistributionQuotient(numerator, denominator);
		}

		private static IEnumerable<int> GetMaxTopicDocs(int topicNumber, string topicsFile)
		{
			var docs = ReadTopicFrom(topicNumber, topicsFile, (doc, nTopic) => Math.Abs(doc[nTopic] - doc.Max()) < 0.001);
			return docs;
		}

		public SortedDictionary<string, double> ThresholdTopicProbabilityDocsDistributionInDays(int[] topicNumbers, double threshold, IDictionary<string, MailUser> mailUsers, string userGeo = null)
		{
			var docs = new List<int>();
			foreach (var topicNumber in topicNumbers)
			{
				docs.AddRange(topics[topicNumber]
				              	.Select((t, i) => Tuple.Create(i, t))
				              	.Where(tpl => tpl.Item2 > threshold)
				              	.Select(tpl => tpl.Item1));
			}

			MailUser user;
			var numerator = GetDistribution(docs.Where(d => userGeo == null || (mailUsers.TryGetValue(QuestionList.GetQuestion(questionIds[d]).AuthorEmail, out user) && user.Geo == userGeo))
												.Select(d => QuestionList.GetQuestion(questionIds[d]).DateAdded.ToShortDateString()));
			var denominator = GetDistribution(Questions.Where(q => userGeo == null || (mailUsers.TryGetValue(q.AuthorEmail, out user) && user.Geo == userGeo))
													   .Select(q => q.DateAdded.ToShortDateString()));
			return Utilits.DistributionQuotient(numerator, denominator);
		}

		public SortedDictionary<int, int> MaxThresholdDocsOverTopicsDistribution(double threshold, int[] topicExclusions = null)
		{
			Assert.Greater(topics.Length, 0);
			var docTopics = GetDocTopics(threshold);

			return GetDistribution(topicExclusions != null ? docTopics.Where(t => !topicExclusions.Contains(t)) : docTopics);
		}

		private IEnumerable<int> GetDocTopics(double threshold)
		{
			var docsCount = topics[0].Count;
			var docTopics = new int[docsCount];
			var docItems = new List<Tuple<int, int, double>>();

			for (var i = 0; i < docsCount; i++)
			{
				var i1 = i;
				var topicByDoc = GetTopicByDoc(i1, threshold);
				docItems.Add(Tuple.Create(i1, topicByDoc.Item1, topicByDoc.Item2));

				docTopics[i1] = topicByDoc.Item1;
			}
			return docTopics;
		}

		public IEnumerable<Question> GetQuestionsByTopic(int topicNumber, double threshold = 0.1)
		{
			Assert.That(topicNumber,Is.LessThan(topics.Length));
			return
				GetDocTopics(threshold).Select((t, d) => Tuple.Create(t, d)).Where(t => t.Item1 == topicNumber).Select(
					t => QuestionList.GetQuestion(questionIds[t.Item2]));
		}

		private Tuple<int, double> GetTopicByDoc(int docNumber, double threshold)
		{
			var doc = topics.Select(t => t[docNumber]);
			var topic = doc.Select((t, i) => new {value=t, index=i}).ElementAtMax(v => v.value);
			return topic.value > threshold ? Tuple.Create(topic.index, topic.value) : Tuple.Create(-1, topic.value);
		}

		public Tuple<int, double> GetTopicByQuestionId(long questionId, double threshold)
		{
			return questionsToDocs.ContainsKey(questionId) ? GetTopicByDoc(questionsToDocs[questionId], threshold) : null;
		}

		public Dictionary<long, double> GetQuestionTopicsProbabilitySums(params int[] topicNumbers)
		{
			return topics[topicNumbers[0]].Select((t, i) => Tuple.Create(questionIds[i], topicNumbers.Sum(n => topics[n][i]))).ToDictionary(it => it.Item1, it => it.Item2);
		}
	}

	[TestFixture]
	internal class TopicsDistributions
	{
		private TopicsStatistics topicsStatistics;
		
		[SetUp]
		public void DistributionInit()
		{
			topicsStatistics = new TopicsStatistics(Program.DefaultQuestionList);

			Console.WriteLine("Preparations have been done");
		}

		[Test]
		public void GetDocsOverTopicsDistrib()
		{
			const double threshold = 0.09;
			var topicExclusions = new[]
			                      	{
			                      		1, 6, 7, 11, 13, 15, 16, 17, 21, 24, 34, 36, 38, 41, 47, 49, 50, 51, 57, 60, 61, 64, 65, 76,
			                      		77, 78, 83, 87, 89
			                      	};
			
			var distrib = topicsStatistics.MaxThresholdDocsOverTopicsDistribution(threshold, topicExclusions);
			var converter = new TopicConverter(Program.TopicsWordsFileName);

			File.WriteAllLines(Program.StatisticsDirectory + "DocsOverTopicsDistrib.txt",
			                   distrib.OrderByDescending(it => it.Value).Select(
			                   	it => it.Key + "\t" + String.Join(", ", converter.GetTopicWords(it.Key, 5)) + "\t" + it.Value));
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
		[TestCase(new[] { 26 }, 0.1)]
		public void ThresholdTopicDistribution(int[] topicNumbers, double threshold = 0.1)
		{
			var distr = topicsStatistics
				.ThresholdTopicProbabilityDocsDistributionInDays(topicNumbers, threshold, Program.DefaultMailUsers);

			var weekDistr = (new SortedDictionary<DateTime, double>(distr.ToDictionary(kv => DateTime.Parse(kv.Key), kv => kv.Value))).SumUpToWeeks();

			File.WriteAllText(Program.StatisticsDirectory + "Topic_distributions/" + String.Join("_", topicNumbers) + "_" + threshold.ToString(CultureInfo.InvariantCulture) + ".txt",
							  String.Join("\n", weekDistr.Select(it => it.Key.ToShortDateString() + '\t' + it.Value)));
		}

		[Test, Explicit]
		public void SearchInterestingThresholdDistribs()
		{
			var interesting = new List<Tuple<int, double>>();

			for (var i = 0; i < topicsStatistics.GetTopicsCount(); i++)
			{
				var fluTopicDistrib = 
					topicsStatistics.ThresholdTopicProbabilityDocsDistributionInDays(new[]{i}, 0.1, Program.DefaultMailUsers, "москва");

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
