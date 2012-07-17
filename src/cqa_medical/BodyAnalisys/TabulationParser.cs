using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iveonik.Stemmers;

namespace cqa_medical.BodyAnalisys
{
	class TabulationParser
	{
		public static IStemmer Stemmer = new RussianStemmer();
		public static IEnumerable<TabulationParserUnit> ParseFromFile(String filename)
		{
			var text = File.ReadAllText(filename);
			return Parse(text.Split('\n','\r').Select(s => s.TrimEnd()).Where(s => s != ""));
		}
		private static IEnumerable<TabulationParserUnit> Parse(IEnumerable<string> strings)
		{
			return strings.Select(s => new TabulationParserUnit(s));
		}
	}

	internal class TabulationParserUnit
	{
		public IEnumerable<String> StemmedStrings { get; private set; }
		public int IndicatorAmount { get; private set; }

		public TabulationParserUnit(String text)
		{
			IndicatorAmount = 0;
			while (text[IndicatorAmount] == '\t')
				IndicatorAmount++;
			text = text.Substring(IndicatorAmount);

			var q = text.Split(' ');
			
				StemmedStrings = q.Select(t => String.Join(" ", t.Split('_').Select(TabulationParser.Stemmer.Stem)));
		}
	}

}
