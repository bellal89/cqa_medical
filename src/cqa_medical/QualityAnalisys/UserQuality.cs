using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.QualityAnalisys
{
	class UserQuality
	{
		internal class UserInfo
		{
			public int Rating { get; set; }
			public float Efficiency { get; set; }
			public int NumberOfAnswers { get; set; }
			public DateTime LastAnswerDate { get; set; }
		}

		private readonly QuestionList questionList;

		private readonly Dictionary<string, UserInfo> userInfos = new Dictionary<string, UserInfo>();

		public UserQuality(QuestionList questionList)
		{
			this.questionList = questionList;
			userInfos = CalculateUserInfos();
		}

		private Dictionary<string, UserInfo> CalculateUserInfos()
		{
			var infos = new Dictionary<string, UserInfo>();
			foreach (var answer in questionList.GetAllAnswers())
			{
				if (!infos.ContainsKey(answer.AuthorEmail))
				{
					infos.Add(answer.AuthorEmail, new UserInfo
					                                  	{
					                                  		Efficiency = answer.AuthorEfficiency,
					                                  		Rating = answer.AuthorRating,
					                                  		NumberOfAnswers = 1,
					                                  		LastAnswerDate = answer.DateAdded
					                                  	});
				}
				else
				{
					infos[answer.AuthorEmail].NumberOfAnswers++;
					if (answer.DateAdded > infos[answer.AuthorEmail].LastAnswerDate)
					{
						infos[answer.AuthorEmail].Efficiency = answer.AuthorEfficiency;
						infos[answer.AuthorEmail].Rating = answer.AuthorRating;
						infos[answer.AuthorEmail].LastAnswerDate = answer.DateAdded;
					}
				}
			}
			return infos;
		}

		public Dictionary<string, UserInfo> GetUserInfos()
		{
			return userInfos;
		}

		public float GetUserQuality(string userEmail)
		{
			if (!userInfos.ContainsKey(userEmail))
				return 0;

			return userInfos[userEmail].Efficiency * userInfos[userEmail].NumberOfAnswers;
		}
	}

	[TestFixture]
	public class UserQualityTest
	{
		[Test]
		public void TestDifferentFuncs()
		{
			var userQuality = new UserQuality(Program.DefaultQuestionList);
			SaveSortedUserQuality(userQuality, userQuality.GetUserQuality);
		}

		private static void SaveSortedUserQuality(UserQuality userQuality, Func<string, float> func)
		{
			var quality = userQuality.GetUserInfos().Keys.OrderByDescending(func).ToDictionary(user => user, func);
			File.WriteAllText("Users_" + func.Method.Name, String.Join("\n", quality.Select(entry => entry.Key + "\t" + entry.Value)));
		}
	}
}
