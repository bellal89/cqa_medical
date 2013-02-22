using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamic;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.Statistics
{
	class LogUserActivity
	{
		public string Name { get; set; }

		private readonly Dictionary<int, int> activity = new Dictionary<int, int>(); 
		private readonly List<Question> userQuestions;

		public LogUserActivity(string userName, IEnumerable<Question> userQuestions, int firstCol, int multiplyStep, int lastCol)
		{
			for (var i = firstCol; i <= lastCol; i *= multiplyStep)
				activity[i] = 0;

			Name = userName;
			this.userQuestions = userQuestions.OrderBy(q => q.DateAdded).ToList();
		
			for (var i = 1; i < this.userQuestions.Count; i++)
			{
				var diff = (this.userQuestions[i].DateAdded - this.userQuestions[i - 1].DateAdded).TotalDays;
				var col = firstCol;
				while (col < diff && col <= lastCol)
				{
					col *= multiplyStep;
				}
				activity[col]++;
			}
		}

		public IEnumerable<KeyValuePair<int, int>> GetActivity()
		{
			return activity.OrderBy(it => it.Key);
		}

		public bool AreEqual(LogUserActivity another)
		{
			var anotherActivity = another.GetActivity().ToList();
			return activity.Count == anotherActivity.Count && anotherActivity.All(col => activity.ContainsKey(col.Key) && activity[col.Key] == col.Value);
		}
	}

	class ActivityEqualityComparer : IEqualityComparer<LogUserActivity>
	{
		public bool Equals(LogUserActivity x, LogUserActivity y)
		{
			return x.AreEqual(y);
		}

		public int GetHashCode(LogUserActivity obj)
		{
			var activity = obj.GetActivity().ToList();
			var hCode = activity[0].Value;
			for (var i = 1; i < activity.Count; i++)
			{
				hCode ^= activity[i].Value;
			}
			return hCode.GetHashCode();
		}
	}

	[TestFixture]
	public class UserActivitiesTest
	{
		[Test]
		public void TestActivities()
		{
			const int minQuestionsCount = 1;

			IEqualityComparer<LogUserActivity> comparer = new ActivityEqualityComparer();
			var acts = Program.DefaultQuestionList.GetAllQuestions().GroupBy(q => q.AuthorEmail, (key, qs) => Tuple.Create(key, qs)).Where(it => it.Item2.Count() >= minQuestionsCount).Select(it => new LogUserActivity(it.Item1, it.Item2, 1, 5, 625)).GroupBy(act => act, (key, items) => Tuple.Create(key, items.Count()), comparer).OrderByDescending(it => it.Item2);
			File.WriteAllLines("UserActivitiesGeometric.txt", acts.Select(a => String.Join(" ", a.Item1.GetActivity().Select(col => col.Value))));
		}
	}
}
