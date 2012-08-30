using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Statistics
{
	public class BaseStatistics
	{
		protected readonly QuestionList QuestionList;
		protected readonly Question[] Questions;
		protected readonly Answer[] Answers;
		public static readonly DateTime FirstDate = new DateTime(2011, 9, 26, 1, 2, 3);

		public BaseStatistics(QuestionList questionList)
		{
			QuestionList = questionList;
			Questions = questionList.GetAllQuestions().ToArray();
			Answers = questionList.GetAllAnswers().ToArray();
		}

		protected SortedDictionary<T, int> GetDistribution<T>(IEnumerable<T> data)
		{
			return new DistributionCreator<T>(data).GetData();
		}
	}
}
