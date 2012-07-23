using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.BodyAnalisys
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
			var text = File.ReadAllText(filename);
			return Parse(text.Split('\n','\r').Select(s => s.TrimEnd()).Where(s => s != ""));
		}
		private IEnumerable<TabulationParserUnit> Parse(IEnumerable<string> strings)
		{
			return strings.Select(s => new TabulationParserUnit(s));
		}
	}

	internal class TabulationParserUnit
	{
		public IEnumerable<String> Strings { get; private set; }
		public IEnumerable<String> StemmedStrings { get { return Strings.Select(TabulationParser.Stemmer.Stem); }}
		public int IndicatorAmount { get; private set; }

		public TabulationParserUnit(String text)
		{
			IndicatorAmount = 0;
			while (text[IndicatorAmount] == '\t')
				IndicatorAmount++;
			text = text.Substring(IndicatorAmount);
			Strings = text.Split(' ');
		}
	}

}
