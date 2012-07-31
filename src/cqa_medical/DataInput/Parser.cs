using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;

namespace cqa_medical.DataInput
{
	internal class Parser : IParser
	{
		public Parser(string questionsFileName, string answersFileName)
		{
			QuestionsFileName = questionsFileName;
			AnswersFileName = answersFileName;
		}

		private string QuestionsFileName { get; set; }

		private string AnswersFileName { get; set; }

		#region IParser Members

		public void Parse(Action<Question> addQuestion, Action<Answer> addAnswer)
		{
			IEnumerable<Question> questions = GetValuesFromCSV<Question>(QuestionsFileName);
			foreach (Question question in questions)
			{
				addQuestion(question);
			}

			IEnumerable<Answer> answers = GetValuesFromCSV<Answer>(AnswersFileName);
			foreach (Answer answer in answers)
			{
			    addAnswer(answer);
			}
		}

		#endregion

		private IEnumerable<T> GetValuesFromCSV<T>(string fileName) where T : class
		{
			var f = new FileStream(fileName, FileMode.Open);
			using (var streamReader = new StreamReader(f, Encoding.GetEncoding(1251)))
			{
				var config = new CsvConfiguration
				             	{Quote = (char) 1, Delimiter = ';', HasHeaderRecord = false, UseInvariantCulture = true};
				var csvReader = new CsvReader(streamReader, config);
				var resultsList = csvReader.GetRecords<T>().ToArray();
				return resultsList;
			}
		}
	}

	[TestFixture]
	public class ParserTest
	{
		[Test]
		public void TestCSVParsing()
		{
			var questionList = new QuestionList(Program.QuestionsFileName, Program.AnswersFileName);
			Assert.AreEqual(313101, questionList.GetAllQuestions().ToArray().Length);
			Assert.IsNotEmpty(questionList.GetQuestion(55879373).GetAnswers());
		}
	}
}