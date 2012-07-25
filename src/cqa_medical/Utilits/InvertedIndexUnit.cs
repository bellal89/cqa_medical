using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cqa_medical.Utilits
{
	class InvertedIndexUnit
	{
		public string Word;
		public HashSet<long> Ids;

		public InvertedIndexUnit(string word, IEnumerable<long> ids)
		{
			Word = word;
			Ids =  new HashSet<long>(ids);
		}
	}
}
