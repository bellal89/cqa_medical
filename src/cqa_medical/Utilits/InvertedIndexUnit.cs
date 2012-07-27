using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cqa_medical.Utilits
{
	public class InvertedIndexUnit
	{
		public string Word;
		public HashSet<long> Ids;

		public InvertedIndexUnit(string word, IEnumerable<long> ids)
		{
			Word = word;
			Ids =  new HashSet<long>(ids);
		}

		public InvertedIndexUnit(string formattedString)
		{
			var q = formattedString.Split(' ');
			Word = q[0];
			Ids = new HashSet<long>(q.Skip(1).Select(Int64.Parse));
		}
		public override string ToString()
		{
			return Word + " " + String.Join(" ", Ids);
		}
	}
}
