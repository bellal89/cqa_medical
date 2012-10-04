using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Statistics
{
	class ExpertUsers
	{
		private readonly Statistics statistics;
		private readonly QuestionList ql;

		public ExpertUsers(QuestionList ql)
		{
			statistics = new Statistics(ql);
			this.ql = ql;
		}

		public IEnumerable<UserInfo> UsersFiltered(int minAnswersAmount, double minAuthorRating)
		{
			var answersAmount = statistics.UserActivityInAnswersDistibution();
			var users = ql.GetAllAnswers();
			return
				users
					.Where(u => answersAmount[u.AuthorEmail] > minAnswersAmount)
					.Where(u => u.AuthorEfficiency > minAuthorRating)
					.Select(u => new UserInfo(u.AuthorEmail, u.AuthorEfficiency, answersAmount[u.AuthorEmail]))
					.Distinct();
		}
		public static IEnumerable<UserInfo> GetDefault()
		{
			var ql = Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness");
			var experts = new ExpertUsers(ql).UsersFiltered(10, 0.15);
			var ans = experts.OrderByDescending(u => u.AnswersAmount * u.Rating).Take(1000);
			return ans;
		}
	}

	internal class UserInfo
	{
		public readonly string Email;
		public readonly double Rating;
		public readonly int AnswersAmount;

		public UserInfo(string email, double rating, int answersAmount)
		{
			Email = email;
			Rating = rating;
			AnswersAmount = answersAmount;
		}

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}", Email, Rating, AnswersAmount);
		}

		public bool Equals(UserInfo other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Email, Email) && other.Rating.Equals(Rating) && other.AnswersAmount == AnswersAmount;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (UserInfo)) return false;
			return Equals((UserInfo) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (Email != null ? Email.GetHashCode() : 0);
				result = (result*397) ^ Rating.GetHashCode();
				result = (result*397) ^ AnswersAmount;
				return result;
			}
		}
	}

	[TestFixture]
	internal class ExpertUserFind
	{
		[Test]
		public void Find()
		{
			var ans = ExpertUsers.GetDefault();
			File.WriteAllLines("userInMessages.txt", ans.Select(u => u.ToString()));
		}

		[Test]
		public void SendAnkets()
		{

			var sender = new MailSender("Our Team mail box @mail.ru", "Our Team mail box password", "smtp.mail.ru");


			var mailSubject = "qwe";
			var mailBody = "\r\ntestTestTEST";

			var ans = ExpertUsers.GetDefault();
//			var ans = new[]
//			          	{
//			          		new UserInfo("Send To @mail.ru", 0.5, 15)
//			          	};
			sender.SendALotOfMails(
				ans.Select( u => new MainMailInfo
					(
						u.Email,
						mailSubject,
						mailBody
					))
				);

		}
	}

}
