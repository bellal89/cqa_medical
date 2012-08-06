using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;
using cqa_medical.Utilits;

namespace cqa_medical.DataInput
{
	///<summary>
	///	Формат файла с ответами:
	/// Номер ответа;Номер вопроса;Лучший?;Кто выбрал лучшим;Адрес автора;Кпд;Баллов;Дата добавления;Сумма оценок;Всего оценок;Текст ответа;источник; 
	/// </summary>

	public class Answer
    {
		private static readonly CultureInfo Culture = new CultureInfo("ru")
		{
			NumberFormat = { NumberDecimalDigits = 4, NumberDecimalSeparator = "." },
			DateTimeFormat = { FullDateTimePattern = "yyyy/MM/dd' 'HH':'mm':'ss" }
		};
		[CsvField(Index = 0)]
		public long Id { get; set; }

		[CsvField(Index = 1)]
		public long QuestionId { get; set; }

		[CsvField(Index = 2)]
		public int IsBest { get; set; }

		[CsvField(Index = 3)]
		public string ChosenBestBy { get; set; }

		[CsvField(Index = 4)]
		public string AuthorEmail { get; set; }

		[CsvField(Index = 5)]
		public float AuthorEfficiency { get; set; }

		[CsvField(Index = 6)]
		public int AuthorRating { get; set; }

		[CsvField(Index = 7)]
		public DateTime DateAdded { get; set; }

		[CsvField(Index = 8)]
		public string Rating { get; set; }

		[CsvField(Index = 9)]
		public string ValuesAmount { get; set; }

		[CsvField(Index = 10)]
		public string Text { get; set; }

		[CsvField(Index = 11)]
		public string Source { get; set; }


		public static Answer FormatStringParse(string formattedString)
		{
			var q = formattedString.Split(';');
			return new Answer
			       	{
			       		Id = long.Parse(q[0]),
			       		QuestionId = long.Parse(q[1]),
			       		IsBest = int.Parse(q[2]),
			       		ChosenBestBy = q[3],
			       		AuthorEmail = q[4],
			       		AuthorEfficiency = float.Parse(q[5],Culture),
			       		AuthorRating = int.Parse(q[6]),
						DateAdded = DateTime.Parse(q[7], Culture),
						Rating = q[8],
			       		ValuesAmount = q[9],
			       		Text = q[10],
			       		Source = q[11],
			       	};
		}

		public static string FormatStringWrite(Answer a)
		{
			return String.Join(";", a.Id, a.QuestionId, a.IsBest, a.ChosenBestBy, a.AuthorEmail,
			                   a.AuthorEfficiency.ToString(Culture), a.AuthorRating,
			                   a.DateAdded.ToString("F", Culture), a.Rating, a.ValuesAmount, a.Text, a.Source);
		}
    }
}
