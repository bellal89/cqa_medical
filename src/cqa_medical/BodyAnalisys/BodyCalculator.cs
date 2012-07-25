using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Iveonik.Stemmers;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;

namespace cqa_medical.BodyAnalisys
{
	class BodyCalculator
	{
		private readonly QuestionList questions;
		private readonly BodyPart body;
		
		public BodyCalculator(QuestionList questions, BodyPart body)
		{
			this.questions = questions;
			this.body = body;
		}

		public BodyPart GetBody()
		{
			return body;
		}

		public void CalculateQuestionDistribution()
		{
			var notBodyQuestions = new List<long>();
			var bodyDict = body.ToDictionary();
			foreach (var question in questions.GetAllQuestions())
			{
				var text = question.Title + " " + question.Text;
				var words = Regex.Split(text, @"\W+");

				var bodyWords = words.Where(bodyDict.ContainsKey).Distinct().ToList();
				if (!bodyWords.Any())
				{
					notBodyQuestions.Add(question.Id);
				}
				foreach (var word in bodyWords)
				{
					bodyDict[word].Inc(question.Id);
				}
			}

			Console.WriteLine((0.0 + notBodyQuestions.Count) / questions.GetAllQuestions().Count());

			File.WriteAllText("NotBody.txt", String.Join("\n\n", notBodyQuestions.Select(id =>
			                                                         	{
			                                                         		var q = questions.GetQuestion(id);
			                                                         		return q.Title + "\n" + q.Text;
																		})));
		}
		
	}

	[TestFixture]
	public class GetBodyCalculations
	{
		[Test]
		public void TestCalculation()
		{
			var questionList = Program.ParseAndStem();
			var body = BodyPart.GetBodyPartsFromFile(Program.BodyPartsFileName);
			var calc = new BodyCalculator(questionList, body);
			calc.CalculateQuestionDistribution();
			var newBody = calc.GetBody();
			var allQuestionsCount = questionList.GetAllQuestions().Count();
			File.WriteAllText(Program.StatisticsDirectory + "1_BodyPartsDistribution.txt", newBody.ToExcelString(allQuestionsCount), Encoding.UTF8);
			File.WriteAllText(Program.StatisticsDirectory + "2_BodyPartsDistribution.txt", newBody.ToString(allQuestionsCount), Encoding.UTF8);
		}
	}
}
