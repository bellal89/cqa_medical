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
		protected readonly QuestionList questionList;
		protected readonly Question[] questions;
		protected readonly Answer[] answers;
		public static readonly DateTime FirstDate = new DateTime(2011, 9, 26, 1, 2, 3);

		public BaseStatistics(QuestionList questionList)
		{
			this.questionList = questionList;
			questions = questionList.GetAllQuestions().ToArray();
			answers = questionList.GetAllAnswers().ToArray();
		}

		protected SortedDictionary<T, int> GetDistribution<T>(IEnumerable<T> data)
		{
			return new DistributionCreator<T>(data).GetData();
		}
	}
}
