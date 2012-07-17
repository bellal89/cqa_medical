using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

		private Dictionary<string, BodyPart> getDict(Dictionary<string, BodyPart> dict, BodyPart part)
		{
			foreach (var name in part.Names)
			{
				dict.Add(name, part);
			}
			if (part.GetSubParts().Count == 0)
			{
				return dict;
			}
			foreach (var bodyPart in part.GetSubParts())
			{
				getDict(dict, bodyPart);
			}
			return dict;
		}

		public void CalculateQuestionDistribution()
		{
			var bodyDict = getDict(new Dictionary<string, BodyPart>(), body);
			foreach (var question in questions.GetAllQuestions())
			{
				var words = Regex.Split(question.Title + " " + question.Text, @"\W+");

				foreach (var word in words.Where(bodyDict.ContainsKey).Distinct())
				{
					bodyDict[word].Inc();
				}
			}
		}
		
	}

	[TestFixture]
	public class BodyCalculatorTest
	{
		[Test]
		public void TestCalculation()
		{
			var parser = new Parser("../../Files/QuestionsTest.csv", "../../Files/AnswersTest.csv");
			var questionList = new QuestionList();
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);

			

//			var calc = new BodyCalculator(questionList, body);
//
//			calc.CalculateQuestionDistribution();
//			calc.GetBody();
		}
	}
}
