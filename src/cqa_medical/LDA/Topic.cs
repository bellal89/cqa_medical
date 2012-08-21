using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;

namespace cqa_medical.LDA
{
	internal class Topic
	{
		private readonly List<Tuple<long, double>> distribOverDocs = new List<Tuple<long, double>>();
		private readonly QuestionList questionList;

		public Topic(QuestionList questionList, string docIdsFileName, string topicsFileName, int topicNumber)
		{
			this.questionList = questionList;
			var ids = File.ReadAllLines(docIdsFileName).Select(long.Parse).ToArray();
			using (var f = new StreamReader(topicsFileName))
			{
				var i = 0;
				while (!f.EndOfStream)
				{
					var line = f.ReadLine();
					if (line == null) throw new Exception("Incorrect topics file!");
					var chance = double.Parse(line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ElementAt(topicNumber),
					                          CultureInfo.InvariantCulture);
					distribOverDocs.Add(Tuple.Create(ids[i++], chance));
				}
			}
		}

		public void GetQuestionExperts()
		{
			throw new NotImplementedException();
		}

		public void GetAnswerExperts()
		{
			throw new NotImplementedException();
		}
	}
}
