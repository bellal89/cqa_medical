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
		public Parser(string _questionsFileName, string _answersFileName)
		{
			questionsFileName = _questionsFileName;
			answersFileName = _answersFileName;
		}

		public string questionsFileName { get; set; }

		public string answersFileName { get; set; }

		#region IParser Members

		public void Parse(Action<Question> addQuestion, Action<Answer> addAnswer)
		{
			IEnumerable<Question> questions = GetValuesFromCSV<Question>(questionsFileName);
			foreach (Question question in questions)
			{
				addQuestion(question);
			}

			IEnumerable<Answer> answers = GetValuesFromCSV<Answer>(answersFileName);
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
				List<T> resultsList = csvReader.GetRecords<T>().ToList();
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
			var parser = new Parser("../../Files/qst_25.csv", "../../Files/ans_25.csv");
			var questionList = new QuestionList();
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);
			Assert.AreEqual(313101, questionList.GetAllQuestions().ToArray().Length);
			Assert.IsNotEmpty(questionList.GetQuestion(55879373).GetAnswers());
		}
	}
}