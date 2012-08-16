using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.UtilitsNamespace
{
	class TabulationParser
	{
		public static IStemmer Stemmer;
		public TabulationParser(IStemmer stemmer = null)
		{
			Stemmer = stemmer;
		}

		public IEnumerable<TabulationParserUnit> ParseFromFile(String filename)
		{
			string[] text = File.ReadAllLines(filename);
			return Parse(text.Select(s => s.TrimEnd()).Where(s => s != ""));
		}
		private IEnumerable<TabulationParserUnit> Parse(IEnumerable<string> strings)
		{
			return strings.Select(s => new TabulationParserUnit(s));
		}
	}

	internal class TabulationParserUnit
	{
		public IEnumerable<string> Words { get; private set; }
		public IEnumerable<string> StemmedWords { get { return Words.Select(TabulationParser.Stemmer.Stem); }}
		public int IndicatorAmount { get; private set; }

		public TabulationParserUnit(string text)
		{
			IndicatorAmount = 0;
			while (text[IndicatorAmount] == '\t')
				IndicatorAmount++;
			text = text.Substring(IndicatorAmount);
			Words = text.Split(' ');
		}
	}

}
