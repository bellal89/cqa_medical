using System;

namespace cqa_medical.DataInput
{
	interface IParser
	{
		void Parse(Action<Question> addQuestion, Action<Answer> addAnswer);
	}
}
