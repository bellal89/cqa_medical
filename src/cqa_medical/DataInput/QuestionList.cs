using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace cqa_medical.DataInput
{
    class QuestionList
    {

		/// <summary>
		/// Change 'questions' signature to Dictionary question[id] => Question
		/// </summary>
		protected Dictionary<long, Question> questions = new Dictionary<long, Question>();
        
		public Dictionary<long, Question> GetQuestions()
		{
			return questions;
		}

        public void AddQuestion(Question question)
        {
            questions.Add(question.Id, question);
        }

		/// <exception cref="KeyNotFoundException">Если это ответ на неизвестный вопрос</exception>
		public void AddAnswer(Answer answer)
		{
			if (questions.ContainsKey(answer.QuestionId))
			{
				Question question = questions[answer.QuestionId];
				question.AddAnswer(answer);
			}
		}

		public void SaveTextToFile(string questionsFile, string answersFile)
		{
			var q =  questions.Values;
			var answers = q.SelectMany(t => t.GetAnswers());
			File.WriteAllText(questionsFile, String.Join("\n", questions.Values));
		}
    }
}
