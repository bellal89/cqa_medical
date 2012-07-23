using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;

namespace cqa_medical.BodyAnalisys
{
	class SymptomSearcher
	{
		private MyStemmer stemmer;
		private QuestionList questionList;
		private Dictionary<string, BodyPart> bodyParts;

		public SymptomSearcher(MyStemmer stemmer, QuestionList questionList, BodyPart body)
		{
			this.stemmer = stemmer;
			this.questionList = questionList;
			bodyParts = body.ToDictionary();
		}
	}
}
