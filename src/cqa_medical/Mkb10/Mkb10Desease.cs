using System;
using System.Collections.Generic;

namespace cqa_medical.DataInput
{
	[Serializable]
	public class Mkb10Desease
	{
		public string Name { get; set; }
		public readonly List<Mkb10TradeName> TradeNames;
		public readonly List<string> Synonyms;

		public Mkb10Desease(string name, List<Mkb10TradeName> tradeNames)
		{
			Name = name;
			TradeNames = tradeNames;
			Synonyms = new List<string>();
		}

		public Mkb10Desease(string name, List<Mkb10TradeName> tradeNames, List<string> synonyms)
		{
			Name = name;
			TradeNames = tradeNames;
			Synonyms = synonyms;
		}
	}
}