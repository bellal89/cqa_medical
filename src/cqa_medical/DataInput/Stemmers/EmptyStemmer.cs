using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cqa_medical.DataInput.Stemmers
{
	class EmptyStemmer : IStemmer
	{
		public string Stem(string word)
		{
			return word;
		}
	}
}
