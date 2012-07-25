using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Iveonik.Stemmers;
using NUnit.Framework;
using cqa_medical.DataInput;

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
	public class BodyCalculatorTest
	{
		[Test]
		public void TestCalculation()
		{
			var parser = new Parser(Program.TestQuestionsFileName, Program.TestAnswersFileName);
			var questionList = new QuestionList();
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);
			questionList.StemIt(new RussianStemmer());

			var body = BodyPart.GetBodyPartsFromFile("../../Files/BodyParts.txt");

			var calc = new BodyCalculator(questionList, body);

			calc.CalculateQuestionDistribution();
			var newBody = calc.GetBody();
			Console.WriteLine(newBody.ToString(questionList.GetAllQuestions().Count()));
		}
	}
}
