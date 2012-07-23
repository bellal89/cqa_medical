using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;

namespace cqa_medical.DataInput
{
	/// <summary>
	/// Формат файла с вопросами:
	/// Номер вопроса;Адрес автора;Кпд;Баллов;Дата добавления;Дата закрытия;Подкатегория;Сумма оценок;Всего оценок;Кто выбрал лучшим;Текст вопроса;Описание;Теги 
	///  </summary>
    class Question
	{
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
			return this.answers;
		}

		public void AddAnswer(Answer answer)
		{
			answers.Add(answer);
		}

		public override string ToString()
		{
			return string.Format("Id: {0}, AuthorEmail: {1}, AuthorEfficiency: {2}, AuthorRating: {3}, DateAdded: {4}, DateClosed: {5}, Category: {6}, Rating: {7}, ValuesAmount: {8}, ChosenBestBy: {9}, Title: {10}, Text: {11}, Tags: {12}", Id, AuthorEmail, AuthorEfficiency, AuthorRating, DateAdded, DateClosed, Category, Rating, ValuesAmount, ChosenBestBy, Title, Text, Tags);
		}
	}
}
