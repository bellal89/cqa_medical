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

		public void SaveTextToFile(string questionsFile, string answersFile)
		{
			var q =  questionList.Values;
			var answers = q.SelectMany(t => t.GetAnswers());
			File.WriteAllText(questionsFile, String.Join("\n", questionList.Values));
		}
    }
}
