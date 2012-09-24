using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Statistics
{
	class UserStatistics : BaseStatistics
	{
		private readonly List<MailUser> users;

		public UserStatistics(QuestionList questionList)
			: base(questionList)
		{
			var parser = new MailUserPageParser(Program.MailUsersDirectory);
			users = parser.GetUsers().ToList();
		}

		public SortedDictionary<string, int> GetUserNames()
		{
			return GetDistribution(users.Select(u => u.Name).Where(n => !String.IsNullOrEmpty(n)));
		}
	}


	[TestFixture]
	public class UserStatisticsTest
	{
		[Test]
		public static void TestUserNames()
		{
			var userStatistics = new UserStatistics(Program.DefaultQuestionList);
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
		public static void SaveUserFirstNames()
		{
			const string malePath = "MaleNamesRating.txt";
			const string femalePath = "FemaleNamesRating.txt";

			var userStatistics = new UserStatistics(Program.DefaultQuestionList);
			var userNames =
				userStatistics.GetUserNames().Select(kv => Regex.Split(kv.Key, @"\W+").FirstOrDefault(part => part != "")).Where(
					n => n != null).Select(n => n.ToLower()).GroupBy(
						name => name, (key, ns) => Tuple.Create(key, ns.Count())).OrderByDescending(n => n.Item2);

			var genderDetector = new GenderDetector();
			var genders = userNames.Select(n => Tuple.Create(n.Item1, n.Item2, genderDetector.Detect(n.Item1))).ToList();

			File.WriteAllLines(malePath, genders.Where(n => n.Item3 == GenderDetector.Gender.Male).Select(n => n.Item1 + "\t" + n.Item2)); 
			File.WriteAllLines(femalePath, genders.Where(n => n.Item3 == GenderDetector.Gender.Female).Select(n => n.Item1 + "\t" + n.Item2));
		}
	}
}
