using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.DataInput
{
	[Serializable]
	public class QuestionList
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
		public  QuestionList (string questionsFileName, string answersFileName, IStemmer stemmer = null)
		{
			if (stemmer != null)
			{
				var questionsStemmedFileName = questionsFileName + stemmer+".csv";
				var answersStemmedFileName = answersFileName + stemmer+".csv";
				var areQuestionsStemmed = DataActualityChecker.IsFileActual(questionsStemmedFileName, new[] {questionsFileName});
				var areAnswersStemmed = DataActualityChecker.IsFileActual(answersStemmedFileName, new[] {answersFileName});

				var start = DateTime.Now;
				var parser = new Parser(
					areQuestionsStemmed?questionsStemmedFileName: questionsFileName,
					areAnswersStemmed?answersStemmedFileName:answersFileName);
				parser.Parse(AddQuestion, AddAnswer);
				Console.WriteLine(String.Format("QuestionList Parsing Completed in {0}",
				                                (DateTime.Now - start).TotalSeconds));
				if (!areQuestionsStemmed)
				{
					StemQuestions(stemmer);
					File.WriteAllLines(questionsStemmedFileName, GetAllQuestions().Select(Question.FormatStringWrite), Encoding.GetEncoding(1251));
				}
				if (!areAnswersStemmed)
				{
					StemAnswers(stemmer);
					File.WriteAllLines(answersStemmedFileName, GetAllAnswers().Select(Answer.FormatStringWrite), Encoding.GetEncoding(1251));
				}

			}
			else
			{
				var start = DateTime.Now;
				var parser = new Parser(questionsFileName, answersFileName);
				parser.Parse(AddQuestion, AddAnswer);

				Console.WriteLine(String.Format("QuestionList Parsing Completed in {0}",
												(DateTime.Now - start).TotalSeconds));
			}


		}

		private void StemAnswers(IStemmer stemmer)
		{
			var start = DateTime.Now;
			foreach (var answer in GetAllAnswers())
			{
				answer.Text = String.Join(" ", UtilitsNamespace.Utilits.GetStemmedWords(stemmer, answer.Text));
			}
			Console.WriteLine(String.Format("Answers Stemming Completed in {0}", (DateTime.Now - start).TotalSeconds));
		}

		private void StemQuestions(IStemmer stemmer)
		{
			var start = DateTime.Now;
			foreach (var question in GetAllQuestions())
			{
				question.Text = String.Join(" ", UtilitsNamespace.Utilits.GetStemmedWords(stemmer, question.Text));
				question.Title = String.Join(" ", UtilitsNamespace.Utilits.GetStemmedWords(stemmer, question.Title));
			}
			Console.WriteLine(String.Format("Questions Stemming Completed in {0}", (DateTime.Now - start).TotalSeconds));
		}

		public override int GetHashCode()
		{
			return (questionList != null ? questionList.Keys.Select(k => k.GetHashCode()).Aggregate(0, (i1, i2) => i1 + i2) : 0);
		}
	}

}
