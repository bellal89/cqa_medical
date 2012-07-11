using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace cqa_medical.UtilitsNamespace
{
	[Serializable]
	public class InvertedIndexUnit
	{
		public string Word;
		public HashSet<long> Ids;


		public InvertedIndexUnit(string word, IEnumerable<long> ids)
		{
			Word = word;
			Ids =  new HashSet<long>(ids);
		}
		public InvertedIndexUnit (string formattedString, char delimiter = ' ')
		{
			var q = formattedString.Split(delimiter);
			Word = q[0];
			Ids = new HashSet<long>(q.Skip(1).Select(Int64.Parse));
		}
		public InvertedIndexUnit(){}

		public string ToStringCount(string delimiter = " ")
		{
			return Word + delimiter + Ids.Count;
		}


		public string ToString(string delimiter = " ", string idDelimiter = ",")
		{
			return Word + delimiter + String.Join(idDelimiter, Ids);
		}

		public static InvertedIndexUnit FormatStringParse(string formattedString)
		{
			var q = formattedString.Split(' ');
			var word = q[0];
			var ids = new HashSet<long>(q.Skip(1).Select(Int64.Parse));
			return new InvertedIndexUnit(word, ids);
		}
		

		public static string FormatStringWrite(InvertedIndexUnit unit)
		{
			return unit.Word + " " + String.Join(" ", unit.Ids);
		}

		//		public InvertedIndexUnit(InvertedIndexUnitWrap unit)
		//		{
		//			Word = unit.Word;
		//			Ids = new HashSet<long>(unit.Ids.Split(',').Select(Int64.Parse));
		//		}
	}

//	public class InvertedIndexUnitWrap
//	{
//		[CsvField(Index = 0)]
//		public string Word { get; set; }
//		[CsvField(Index = 1)]
//		public string Ids { get; set; }
//
//		public InvertedIndexUnitWrap (InvertedIndexUnit unit)
//		{
//			Word = unit.Word;
//			Ids = String.Join(",", unit.Ids);
//		}
//		public InvertedIndexUnitWrap (){}
//
//	}
}
