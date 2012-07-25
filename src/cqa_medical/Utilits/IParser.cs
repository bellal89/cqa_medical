using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;

namespace cqa_medical.Utilits
{
	interface IParser
	{
		void Parse(Action<Question> addQuestion, Action<Answer> addAnswer);
	}
}
