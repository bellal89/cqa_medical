﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.LDA;
using cqa_medical.UtilitsNamespace;
using cqa_medical.UtilitsNamespace.Parsers;

namespace cqa_medical.Statistics
{
	class UserStatistics : BaseStatistics
	{
		private readonly List<MailUser> users;
		public readonly TopicsStatistics TopicStatistics;

		public UserStatistics(QuestionList questionList)
			: base(questionList)
		{
			var parser = new MailUserPageParser(Program.MailUsersDirectory);
			
			var questionUsers = questionList.GetAllQuestions().Select(q => q.AuthorEmail);
			var answerUsers = questionList.GetAllAnswers().Select(a => a.AuthorEmail);
			var questionListUsers = new HashSet<string>(questionUsers.Union(answerUsers));

			users = parser.GetObjects().Where(u => questionListUsers.Contains(u.Email)).ToList();

			TopicStatistics = new TopicsStatistics(questionList);
		}

		public List<MailUser> GetUsers()
		{
			return users;
		}

		public SortedDictionary<string, int> GetUserNames()
		{
			return GetDistribution(users.Select(u => u.Name).Where(n => !String.IsNullOrEmpty(n)));
		}

		public SortedDictionary<string, int> GetUsersByRegionDistribution()
		{
			var cities = Cities.GetRussianCities();
			return GetDistribution(users.Where(u => u.Geo != null).Select(u => cities.GetDomain(u.Geo)).Where(domain => domain != null));
		}

		public SortedDictionary<string, int> GetUsersByTopicByRegionDistribution(int[] topicNumbers, string region, double threshold)
		{
			var regionTopicsQuestions = new List<Question>();

			foreach (var number in topicNumbers)
			{
				regionTopicsQuestions.AddRange(GetQuestionsByTopicByRegion(number, region, threshold).ToList());
			}
			
			var userList = QuestionList.GetAllQuestions().Select(q => q.AuthorEmail).Distinct().ToList();
			Console.WriteLine(userList.Count);

			var regionEmails = GetRegionEmails("московская область");
			Console.WriteLine(userList.Where(regionEmails.Contains).Count());

			Console.WriteLine("QuestionsList size = " + regionTopicsQuestions.Select(q => q.Id).Distinct().Count());
			return GetDistribution(regionTopicsQuestions.Select(q => q.DateAdded.GetWeek().ToShortDateString()));
		}

		private IEnumerable<Question> GetQuestionsByTopicByRegion(int topicNumber, string region, double threshold)
		{
			var regionEmails = GetRegionEmails(region);

			var regionTopicQuestions =
				TopicStatistics.GetQuestionsByTopic(topicNumber, threshold).Where(q => regionEmails.Contains(q.AuthorEmail));
			return regionTopicQuestions;
		}

		private HashSet<string> GetRegionEmails(string region)
		{
			var regionCities = Cities.GetRussianCities().GetAllCitiesInDomain(region);
			regionCities.Add(region);

			return new HashSet<string>(users.Where(u => u.Geo != null && regionCities.Contains(u.Geo)).Select(u => u.Email));
		}

		public SortedDictionary<int, int> GetRegionUsersByTopicsDistribution(string region, double threshold)
		{
			var regionEmails = GetRegionEmails(region);

			return
				GetDistribution(
					QuestionList.GetAllQuestions().Where(q => regionEmails.Contains(q.AuthorEmail)).Select(
						q => TopicStatistics.GetTopicByQuestionId(q.Id, threshold)).Where(t => t != null).Select(t => t.Item1));
		}

		public Dictionary<DateTime, double> GetTopicByUserRegionProbabilitiesDistribution(string region, params int[] topics)
		{
			var regionEmails = GetRegionEmails(region);
			var topicsProbabilitySums = TopicStatistics.GetQuestionTopicsProbabilitySums(topics);

			return QuestionList.GetAllQuestions()
				.Where(q => regionEmails.Contains(q.AuthorEmail) && topicsProbabilitySums.ContainsKey(q.Id))
				.GroupBy(q => q.DateAdded.Date, (key, qs) => Tuple.Create(key, qs.Sum(q => topicsProbabilitySums[q.Id]) / qs.Count()))
				.ToDictionary(it => it.Item1, it => it.Item2);
		}

		public List<Question> GetQuestionsByRegion(string region)
		{
			var regionEmails = GetRegionEmails(region);
			return QuestionList.GetAllQuestions().Where(q => regionEmails.Contains(q.AuthorEmail)).ToList();
		}

		public SortedDictionary<DateTime, int> GetRegionQuestionsDistrib(string region)
		{
			return GetDistribution(GetQuestionsByRegion(region).Select(q => q.DateAdded.Date));
		}

		public Dictionary<DateTime, List<double>> GetAllWeekRegionTopicProbabilities(string region)
		{
			var allTopicProbabilities = new Dictionary<DateTime, List<double>>();

			for (var i = 0; i < Program.TopicsCount; i++)
			{
				var distrib = GetTopicByUserRegionProbabilitiesDistribution(region, i).SumUpToWeeks();
				foreach (var key in distrib.Keys)
				{
					if (!allTopicProbabilities.ContainsKey(key))
						allTopicProbabilities[key] = new List<double>();
					allTopicProbabilities[key].Add(distrib[key]);
				}
			}
			foreach (var d in allTopicProbabilities)
			{
				Assert.AreEqual(Program.TopicsCount, d.Value.Count);
			}

			return allTopicProbabilities;
		}

		public SortedDictionary<int, int> GetBestAnswerTimeDistribution()
		{
			var illnessQuestions = Program.DefaultNotStemmedQuestionList.NewQuestionListFilteredByCategories("illness");
			return GetDistribution(
				illnessQuestions.GetAllQuestions().Select(
					q =>
						{
							var answers = q.GetAnswers().Where(a => a.IsBest != 0);
							if (!answers.Any())
								return 0;
							return (int) ((answers.First().DateAdded - q.DateAdded).TotalMinutes);
						})
						);
		}
	}


	[TestFixture]
	public class UserStatisticsTest
	{
	    private readonly QuestionList medicalQuestions =
	        Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness",
	                                                                        "treatment",
	                                                                        "kidhealth",
	                                                                        "doctor");
        private UserStatistics userStatistics;
        const string Region = "московская область";
        
        [SetUp]
        public void Init()
        {
            userStatistics = new UserStatistics(medicalQuestions);
        }

	    [Test]
		public void TestUserNames()
		{
			var userNames = userStatistics.GetUserNames().OrderByDescending(u => u.Value);
			var genderDetector = new GenderDetector();
			var genders = userNames.Select(n => Tuple.Create(n.Key, genderDetector.Detect(n.Key))).ToList();
			Console.WriteLine("Male %: " + (double)genders.Count(g => g.Item2 == GenderDetector.Gender.Male) / genders.Count);
			Console.WriteLine("Female %: " + (double)genders.Count(g => g.Item2 == GenderDetector.Gender.Female) / genders.Count);
			Console.WriteLine("Unknown %: " + (double)genders.Count(g => g.Item2 == GenderDetector.Gender.Unknown) / genders.Count);

			Console.WriteLine(userNames.Sum(u => u.Value));
			Console.WriteLine("Unknowns");
			Console.WriteLine(String.Join("\n", genders.Where(g => g.Item2 == GenderDetector.Gender.Unknown).Select(u => u.Item1)));
//			Console.WriteLine("Males");
//			Console.WriteLine(String.Join("\n", genders.Where(g => g.Item2 == GenderDetector.Gender.Male).Select(u => u.Item1)));
//			Console.WriteLine("Females");
//			Console.WriteLine(String.Join("\n", genders.Where(g => g.Item2 == GenderDetector.Gender.Female).Select(u => u.Item1)));
		}

		[Test, Explicit]
		public void SaveUserFirstNames()
		{
			const string malePath = "MaleNamesRating.txt";
			const string femalePath = "FemaleNamesRating.txt";

			var userNames =
				userStatistics.GetUserNames().Select(kv => Regex.Split(kv.Key, @"\W+").FirstOrDefault(part => part != "")).Where(
					n => n != null).Select(n => n.ToLower()).GroupBy(
						name => name, (key, ns) => Tuple.Create(key, ns.Count())).OrderByDescending(n => n.Item2);

			var genderDetector = new GenderDetector();
			var genders = userNames.Select(n => Tuple.Create(n.Item1, n.Item2, genderDetector.Detect(n.Item1))).ToList();

			File.WriteAllLines(malePath, genders.Where(n => n.Item3 == GenderDetector.Gender.Male).Select(n => n.Item1 + "\t" + n.Item2)); 
			File.WriteAllLines(femalePath, genders.Where(n => n.Item3 == GenderDetector.Gender.Female).Select(n => n.Item1 + "\t" + n.Item2));
		}

		[Test]
		public void QuickStatistics()
		{
			Console.WriteLine("Grls count: " + File.ReadAllLines(Program.MedicamentsFileName).Distinct().Count());

			var illnessQuestions = Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness");
			var bestAnswerTimes = 
			illnessQuestions.GetAllQuestions().Select(
				q =>
					{
						var answers = q.GetAnswers().Where(a => a.IsBest != 0);
						if (!answers.Any())
							return 0;
						return (int) ((answers.First().DateAdded - q.DateAdded).TotalMinutes);
					}).OrderBy(minutes => minutes).ToList();

			var medianTime = bestAnswerTimes[bestAnswerTimes.Count / 2];

			Console.WriteLine("Time median to best answer choosing: " + medianTime);

			var chosenBest = illnessQuestions.GetAllQuestions().GroupBy(q => q.ChosenBestBy, (key, qs) => Tuple.Create(key, qs.Count()));
			
			foreach (var mark in chosenBest)
			{
				Console.WriteLine(mark.Item1 + ": " + mark.Item2 + ";  " + (double)mark.Item2 / illnessQuestions.GetAllQuestions().Count());
			}

			Console.WriteLine("Illness questions count: " + illnessQuestions.GetAllQuestions().Count());

			var questerEmails = new HashSet<string>(illnessQuestions.GetAllQuestions().Select(q => q.AuthorEmail));
			var answererEmails = new HashSet<string>(illnessQuestions.GetAllAnswers().Select(a => a.AuthorEmail));
			
			var illnessEmails = new HashSet<string>(questerEmails.Union(answererEmails));
			Console.WriteLine("All illness users: " + illnessEmails.Count);
			
			var questerProfilesCount = userStatistics.GetUsers().Count(u => questerEmails.Contains(u.Email));
			var answererProfilesCount = userStatistics.GetUsers().Count(u => answererEmails.Contains(u.Email));
			var illnessUsers =
				userStatistics.GetUsers().Where(u => questerEmails.Contains(u.Email) || answererEmails.Contains(u.Email)).ToList();

			Console.WriteLine("Questions asked by: " + questerEmails.Count);
			Console.WriteLine("Answers asked by: " + answererEmails.Count);
			Console.WriteLine("Public profiles gathered for: " + illnessUsers.Count);

			Console.WriteLine("Age pointed out: " + (double)illnessUsers.Count(u => u.BirthDate.Year > 1900) / illnessUsers.Count);
			Console.WriteLine("GeoLoc pointed out: " + (double)illnessUsers.Count(u => u.Geo != null) / illnessUsers.Count);
		}

		[Test, Explicit]
		public void SaveUsersByRegionDistribution()
		{
			const string fileToSave = "";

			var userRegions = userStatistics.GetUsersByRegionDistribution().OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);

			Console.WriteLine(userStatistics.GetUsers().Count);
			Console.WriteLine(userStatistics.GetUserNames().Count);
			
			var usersWithRegion = userRegions.Sum(r => r.Value);
			Console.WriteLine("Users from Moscow: " + (double)userRegions[Region] / usersWithRegion);

			Console.WriteLine(String.Join("\n", userRegions.Select(kv => kv.Key + "\t" + kv.Value)));
		}

		[Test, Explicit]
		[TestCase(0.08, "московская область", 26, 53, 98)]
		[TestCase(0.08, "московская область", new[]{86})]
		public void SaveUsersDistrib(double threshold, string region, params int[] topics)
		{
			File.WriteAllLines("Topic_" + String.Join("_", topics) + "_Region_" + region + ".txt",
			                   userStatistics.GetUsersByTopicByRegionDistribution(topics, region, threshold)
							   .OrderByDescending(u => u.Value).Select(u => u.Key + "\t" + u.Value));
		}

		[Test]
		public void RegionUserTopicDistrib()
		{
			Console.WriteLine(String.Join("\n", userStatistics.GetRegionUsersByTopicsDistribution("московская область", 0.1).OrderByDescending(kv => kv.Value).Select(kv => kv.Key + "\t" + kv.Value)));
		}

		[Test, Explicit]
		public void TestTopicByUserRegionProbabilitiesDistribution()
		{
			
			var topics = new[]
			             	{
			             		//26, 53, 95, 98
								//46, 67, 75
								//35
								98
							};
			
			var topicsProbabilities = userStatistics.GetTopicByUserRegionProbabilitiesDistribution(Region, topics).SumUpToWeeks().OrderByDescending(it => it.Value).Select(it => it.Key + "\t" + it.Value);
			
			File.WriteAllLines("MoscowTopicProbability_"+ String.Join("_", topics) + ".txt", topicsProbabilities);
		}

		[Test]
		public void TestAlltopicProbabilities()
		{
			var values = userStatistics.GetAllWeekRegionTopicProbabilities(Region);
			File.WriteAllLines("MoscowAllTopicsProbabilitiesNormalized.txt", values.Select(v => v.Key + "\t" + String.Join("\t", v.Value)), Encoding.UTF8);
		}

		[Test]
		public void TestUserRegionQuestionsAmountDistrib()
		{

			File.WriteAllLines("MoscowQuestionsAmountDistrib.txt", userStatistics.GetRegionQuestionsDistrib(Region).SumUpToWeeks().Select(it => it.Key + "\t" + it.Value));

		}

		[Test, Explicit]
		public void GetCorrelations()
		{
			// 4368 = Moscow code
			var collection = new GismeteoWeatherCollection(4368, new DateTime(2011, 04, 3), new DateTime(2012, 03, 31));

			var distribs = new List<Tuple<string, Dictionary<DateTime, double>>>
			    {
			        Tuple.Create("Cloudiness", collection.GetWeekCloudinessDistribution().ToDictionary(it => it.Key, it => (double) it.Value)),
			        Tuple.Create("Rain", collection.GetWeekRainDistribution().ToDictionary(it => it.Key, it => (double) it.Value)),
			        Tuple.Create("EveningTemperature", collection.GetWeekEveningTemperature()),
			        Tuple.Create("DayPressure", collection.GetWeekPressureDistribution())
			    };

			foreach (var d in distribs)
			{
				Console.WriteLine("\n" + d.Item1);
				PrintTopCorrelations(d.Item2.NormalizeByMax(), 10);
			}
		}

		private void PrintTopCorrelations(IDictionary<DateTime, double> cloudinessDistrib, int topCount)
		{
			var allTopicProbabilities = userStatistics.GetAllWeekRegionTopicProbabilities(Region);

			var topicConverter = new TopicConverter(Program.TopicsWordsFileName);

			var corrs = new List<Tuple<int, double>>();
			for (var i = 0; i < Program.TopicsCount; i++)
			{
				corrs.Add(Tuple.Create(i,
				                       Utilits.GetTimeCorrelation(
				                       	allTopicProbabilities.ToDictionary(p => p.Key, p => p.Value[i]),
				                       	cloudinessDistrib)));
			}

			Console.WriteLine(String.Join("\n",
			                              corrs.OrderByDescending(cor => Math.Abs(cor.Item2)).Take(topCount).Select(
											cor => cor.Item1 + "\t" + String.Join(", ", topicConverter.GetTopicWords(cor.Item1, 4)) + "\t" + cor.Item2)));
		}

        [Test, Explicit("SaveAllStatistics")]
        public void SaveAllStatistics()
        {
            var genderDetector = new GenderDetector();
            var users = userStatistics.GetUsers().ToDictionary(u => u.Email, u => u);
            File.WriteAllLines("FullQuestionStatistics.txt",
                               medicalQuestions.GetAllQuestions().Select(
                                   a =>
                                       {
                                           var topic = userStatistics.TopicStatistics.GetTopicByQuestionId(a.Id, 0.1) ??
                                                       Tuple.Create(-1, 0.0);
                                           return a.Id + "\t" + 0 + "\t" +
                                                  a.Category + "\t" + a.Rating +
                                                  "\t" +
                                                  a.DateAdded + "\t" +
                                                  topic.Item1 + "\t" + topic.Item2.ToString(CultureInfo.InvariantCulture) + "\t" +
                                                  a.WholeText +
                                                  "\t" +
                                                  a.AuthorEmail + "\t" + a.AuthorRating + "\t" + a.AuthorEfficiency.ToString(CultureInfo.InvariantCulture) +
                                                  "\t" +
                                                  (users.ContainsKey(a.AuthorEmail)
                                                       ? (users[a.AuthorEmail].BirthDate + "\t" +
                                                          users[a.AuthorEmail].Geo +
                                                          "\t" +
                                                          (String.IsNullOrEmpty(users[a.AuthorEmail].Name)
                                                               ? "Unknown"
                                                               : genderDetector.Detect(users[a.AuthorEmail].Name).
                                                                     ToString()))
                                                       : "\t\t");
                                       }));
        }
	}
}
