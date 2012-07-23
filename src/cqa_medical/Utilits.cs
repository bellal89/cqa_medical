using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Iveonik.Stemmers;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical
{
	internal static class Utilits
	{
		public static string StripHTMLTags(this String s)
		{
			return Regex.Replace(s, "<[^>]*?>", string.Empty, RegexOptions.IgnoreCase);
		}

		public static IEnumerable<string> SplitIntoWords(this string s)
		{
			return Regex.Split(s, @"\W+").Where(t => t != "");
		}

		public static TValue GetOrDefault<TKey, TValue>(this SortedDictionary<TKey, TValue> dict, TKey key,
		                                                TValue defaultValue)
		{
			return dict.ContainsKey(key) ? dict[key] : defaultValue;
		}

		public static string ToStringNormal<TKey, TValue>(this SortedDictionary<TKey, TValue> data)
		{
			return string.Join(Environment.NewLine, data.Keys.Select(k => k.ToString() + "\t" + data[k].ToString()).ToArray());
		}

		public static string ToStringInverted<TKey, TValue>(this SortedDictionary<TKey, TValue> data)
		{
			return string.Join(Environment.NewLine, data.Keys.Select(k => data[k].ToString() + "\t" + k.ToString()).ToArray());
		}


		public static string[] GetStemmedStrings(IStemmer stemmer, string text)
		{
			var noHTMLWords = text.StripHTMLTags().SplitIntoWords();
			var words = noHTMLWords.Select(stemmer.Stem).ToArray();
			return words;
		}

		public static SortedDictionary<TKey, double> DistributientQuotient<TKey>(SortedDictionary<TKey, int> numerator,
		                                                                  SortedDictionary<TKey, int> denominator)
		{
			var result = new SortedDictionary<TKey, double>();
			foreach (var w in numerator.Where(w => denominator.ContainsKey(w.Key)))
				result.Add(w.Key, (double) w.Value/denominator[w.Key]);
			return result;
		}
	}
}
