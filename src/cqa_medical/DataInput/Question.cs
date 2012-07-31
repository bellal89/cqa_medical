using System;
using System.Collections.Generic;
using System.Globalization;
using CsvHelper.Configuration;
using cqa_medical.Utilits;

namespace cqa_medical.DataInput
{
	/// <summary>
	/// Формат файла с вопросами:
	/// Номер вопроса;Адрес автора;Кпд;Баллов;Дата добавления;Дата закрытия;Подкатегория;Сумма оценок;Всего оценок;Кто выбрал лучшим;Текст вопроса;Описание;Теги 
	///  </summary>
	public class Question : IFormatParse<Question>
	{

		private static readonly CultureInfo Culture = new CultureInfo("ru")
		{
			NumberFormat = { NumberDecimalDigits = 4, NumberDecimalSeparator = "." },
			DateTimeFormat = { FullDateTimePattern = "yyyy/MM/dd' 'HH':'mm':'ss" }
		};
		[CsvField(Index = 0)]
		public long Id { get; set; }

		[CsvField(Index = 1)]
		public string AuthorEmail { get; set; }

		[CsvField(Index = 2)]
		public float AuthorEfficiency { get; set; }

		[CsvField(Index = 3)]
		public long AuthorRating { get; set; }

		[CsvField(Index = 4)]
		public DateTime DateAdded { get; set; }

		[CsvField(Index = 5)]
		public DateTime DateClosed { get; set; }

		[CsvField(Index = 6)]
		public string Category { get; set; }

		[CsvField(Index = 7)]
		public string Rating { get; set; }

		[CsvField(Index = 8)]
		public string ValuesAmount { get; set; }

		[CsvField(Index = 9)]
		public string ChosenBestBy { get; set; }

		[CsvField(Index = 10)]
		public string Title { get; set; }

		[CsvField(Index = 11)]
		public string Text { get; set; }

		[CsvField(Index = 12)]
		public string Tags { get; set; }

		[CsvField(Ignore = true)]
		public string WholeText { get { return Title + " " + Text; } }

		private readonly List<Answer> answers = new List<Answer>();

		public List<Answer> GetAnswers()
		{
			return answers;
		}

		public void AddAnswer(Answer answer)
		{
			answers.Add(answer);
		}
		
		public Question FormatStringParse(string formattedString)
		{
			var q = formattedString.Split(';');
			return new Question
			       	{
			       		Id = long.Parse(q[0]),
			       		AuthorEmail = q[1],
			       		AuthorEfficiency = float.Parse(q[2]),
			       		AuthorRating = long.Parse(q[3]),
			       		DateAdded = DateTime.Parse(q[4]),
			       		DateClosed = DateTime.Parse(q[5]),
			       		Category = q[6],
			       		Rating = q[7],
			       		ValuesAmount = q[8],
			       		ChosenBestBy = q[9],
			       		Title = q[10],
			       		Text = q[11],
			       		Tags = q[12],
			       	};
		}

		public string FormatStringWrite()
		{
			return string.Join(";", Id, AuthorEmail, AuthorEfficiency.ToString(Culture),AuthorRating,
				DateAdded.ToString("F", Culture), DateClosed.ToString("F", Culture),
				Category, Rating, ValuesAmount, ChosenBestBy, Title, Text, Tags);
		}

		public override string ToString()
		{
			return string.Format("Id: {0}, AuthorEmail: {1}, AuthorEfficiency: {2}, AuthorRating: {3}, DateAdded: {4}, DateClosed: {5}, Category: {6}, Rating: {7}, ValuesAmount: {8}, ChosenBestBy: {9}, Title: {10}, Text: {11}, Tags: {12}", Id, AuthorEmail, AuthorEfficiency, AuthorRating, DateAdded, DateClosed, Category, Rating, ValuesAmount, ChosenBestBy, Title, Text, Tags);
		}
	}
}

