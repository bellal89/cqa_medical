using System;
using System.Collections;
using System.Collections.Generic;
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
        
        public void AddQuestion(Question question)
        {
            questions.Add(question.Id, question);
        }

		/// <exception cref="KeyNotFoundException">Если это ответ на неизвестный вопрос</exception>
		public void AddAnswer(Answer answer)
		{
			Question question = questions[answer.QuestionId];
			question.AddAnswer(answer);
		}
    }
}
