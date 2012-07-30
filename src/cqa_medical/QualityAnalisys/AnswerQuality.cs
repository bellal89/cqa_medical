using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.QualityAnalisys
{
	class AnswerQuality
	{
		private readonly UserQuality userQuality;
		private readonly QuestionList questionList;
		private readonly Dictionary<long, Answer> answers = new Dictionary<long, Answer>();

		public AnswerQuality(QuestionList questionList)
		{
			this.questionList = questionList;
			userQuality = new UserQuality(questionList);
			foreach (var answer in questionList.GetAllAnswers().Where(answer => !answers.ContainsKey(answer.Id)))
			{
				answers.Add(answer.Id, answer);
			}
		}

		public Dictionary<long, Answer> GetAnswers()
		{
			return answers;
		}

		public float GetAnswerQuality(long id)
		{
			if (!answers.ContainsKey(id)) return 0;
			var result = userQuality.GetUserQuality(answers[id].AuthorEmail) + 200*answers[id].IsBest;
			if (questionList.GetQuestion(answers[id].QuestionId).Category == "doctor")
				result += 2000;
			return result;
		}
	}
	
	[TestFixture]
	public class AnswersQualityTest
	{
		[Test]
		public void TestAnswersQuality()
		{
			var answersQuality = new AnswerQuality(Program.DefaultQuestionList);
			SaveSortedUserQuality(answersQuality, answersQuality.GetAnswerQuality);
		}

		private static void SaveSortedUserQuality(AnswerQuality answerQuality, Func<long, float> func)
		{
			var quality =
				answerQuality.GetAnswers().OrderByDescending(a => func(a.Key)).Select(
					a => a.Value.QuestionId + "\t" + a.Key + "\t" + func(a.Key) + "\t" + Program.DefaultQuestionList.GetQuestion(a.Value.QuestionId).Category);
			File.WriteAllText("Answers_" + func.Method.Name, String.Join("\n", quality));
		}
	}
}
