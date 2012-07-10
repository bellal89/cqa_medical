using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cqa_medical.DataInput
{
	/// <summary>
	/// Формат файла с вопросами:
	/// Номер вопроса;Адрес автора;Кпд;Баллов;Дата добавления;Дата закрытия;Подкатегория;Сумма оценок;Всего оценок;Кто выбрал лучшим;Текст вопроса;Описание;Теги 
	///  </summary>
    class Question
	{
		public long Id { get; private set; }

		public string AuthorEmail { get; set; }

		public float AuthorEfficiency { get; set; }

		public int AuthorRating { get; set; }

		public DateTime DateAdded { get; set; }

		public DateTime DateClosed { get; set; }

		public string Category { get; set; }

		public int Rating { get; set; }

		public int ValuesAmount { get; set; }

		public string ChosenBestBy { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public List<string> Tags { get; set; }

		private List<Answer> answers;

    	public Question(Int64 id)
    	{
    		this.Id = id;
			// заполнить текст и остальные поля
    	}

		public List<Answer> GetAnswers()
		{
			return this.answers;
		}

		public void AddAnswer(Answer answer)
		{
			answers.Add(answer);
		}
	}
}
