using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;

namespace cqa_medical.DataInput
{
	///<summary>
	///	Формат файла с ответами:
	/// Номер ответа;Номер вопроса;Лучший?;Кто выбрал лучшим;Адрес автора;Кпд;Баллов;Дата добавления;Сумма оценок;Всего оценок;Текст ответа;источник; 
	/// </summary>

    public class Answer
    {
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
    }
}
