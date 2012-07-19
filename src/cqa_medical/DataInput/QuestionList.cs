using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iveonik.Stemmers;
using cqa_medical;

namespace cqa_medical.DataInput
{
	internal class QuestionList
	{

		/// <summary>
		/// Change 'questionList' signature to Dictionary questionList[id] => Question
		/// </summary>
		private readonly Dictionary<long, Question> questionList = new Dictionary<long, Question>();

		public IEnumerable<Question> GetAllQuestions()
		{
			return questionList.Values;
		}

		public IEnumerable<Answer> GetAllAnswers()
		{
			return GetAllQuestions().SelectMany(t => t.GetAnswers());
		}

		public Question GetQuestion(long id)
		{
			return questionList[id];
		}

		public void AddQuestion(Question question)
		{
			questionList.Add(question.Id, question);
		}

		/// <exception cref="KeyNotFoundException">Если это ответ на неизвестный вопрос</exception>
		public void AddAnswer(Answer answer)
		{
			if (questionList.ContainsKey(answer.QuestionId))
			{
				Question question = questionList[answer.QuestionId];
				question.AddAnswer(answer);
			}
		}



		/// <summary>
		/// Use SnowballStemmer to modify text of messages
		/// </summary>
		/// <returns>QuestionList</returns>
		public QuestionList StemIt()
		{
			var stemmer = new RussianStemmer();
			DateTime start;

			start = DateTime.Now;
			foreach (var question in GetAllQuestions())
			{
				question.Text = String.Join(" ", Utilits.GetStemmedStrings(stemmer, question.Text));
				question.Title = String.Join(" ", Utilits.GetStemmedStrings(stemmer, question.Title));
			}
			Console.WriteLine(String.Format("Questions Stemming Completed in {0}", (DateTime.Now - start).TotalSeconds));

			start = DateTime.Now;
			foreach (var answer in GetAllAnswers())
			{
			    answer.Text = String.Join(" ", Utilits.GetStemmedStrings(stemmer, answer.Text));
			}
			Console.WriteLine(String.Format("Answers Stemming Completed in {0}", (DateTime.Now - start).TotalSeconds));
			return this;
		}
	}
}
