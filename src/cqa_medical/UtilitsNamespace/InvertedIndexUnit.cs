using System;
using System.Collections.Generic;
using System.Linq;

namespace cqa_medical.UtilitsNamespace
{
	public class InvertedIndexUnit
	{
		public readonly string Word;
		public readonly HashSet<long> Ids;

		public InvertedIndexUnit(string word, IEnumerable<long> ids)
		{
			Word = word;
			Ids =  new HashSet<long>(ids);
		}

		public InvertedIndexUnit(string formattedString, char idsDelimiter = ' ')
		{
			var q = formattedString.Split(idsDelimiter);
			Word = q[0];
			Ids = new HashSet<long>(q.Skip(1).Select(Int64.Parse));
		}

		public string ToStringCount(string delimiter = " ")
		{
			return Word + delimiter + Ids.Count;
		}

		public string ToString(string delimiter = " ", string idDelimiter = ",")
		{
			return Word + delimiter + String.Join(idDelimiter, Ids);
		}
		
		public override string ToString()
		{
			return ToString();
		}

		public static InvertedIndexUnit FormatStringParse(string formattedString)
		{
			return new InvertedIndexUnit(formattedString);
		}
		
		public static string FormatStringWrite(InvertedIndexUnit unit)
		{
			return unit.Word + " " + String.Join(" ", unit.Ids);
		}

	}

}
