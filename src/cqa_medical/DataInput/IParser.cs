using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cqa_medical.DataInput
{
	interface IParser
	{
		void Parse(Action<Question> addQuestion, Action<Answer> addAnswer);
	}
}
