using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cqa_medical.DataInput
{
	///<summary>
	///	Формат файла с ответами:
	/// Номер ответа;Номер вопроса;Лучший?;Кто выбрал лучшим;Адрес автора;Кпд;Баллов;Дата добавления;Сумма оценок;Всего оценок;Текст ответа;источник; 
	/// </summary>

    class Answer
    {
		public long Id { get; private set; }
		
		public long QuestionId { get; private set; }

		public bool IsBest { get; set; }

		public string ChosenBestBy { get; set; }

		public string AuthorEmail { get; set; }

		public float AuthorEfficiency { get; set; }

		public int AuthorRating { get; set; }

		public DateTime DateAdded { get; set; }

		public int Rating { get; set; }

		public int ValuesAmount { get; set; }

		public string Text { get; set; }

		public string Source { get; set; }

		public Answer(long id, long questionId)
		{
			Id = id;
			QuestionId = questionId;
		}

    }
}
