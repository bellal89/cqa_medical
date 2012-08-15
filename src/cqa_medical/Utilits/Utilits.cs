using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.Utilits
{
	internal static class Utilits
	{
		public static string StripHTMLTags(this String s)
		{
			return Regex.Replace(s, "<[^>]*?>", string.Empty, RegexOptions.IgnoreCase);
		}

		public static IEnumerable<string> SplitIntoWords(this string s)
		{
			return Regex.Split(s.ToLower(), @"\W+").Where(t => t != "");
		}

		public static IEnumerable<string> SplitInWordsAndStripHTML(this string s)
		{
			return s.StripHTMLTags().SplitIntoWords();
		}

		public static TValue GetOrDefault<TKey, TValue>(this SortedDictionary<TKey, TValue> dict, TKey key,
		                                                TValue defaultValue)
		{
			return dict.ContainsKey(key) ? dict[key] : defaultValue;
		}

		public static string ToStringNormal<TKey, TValue>(this IDictionary<TKey, TValue> data)
		{
			return string.Join(Environment.NewLine, data.Keys.Select(k => k.ToString() + "\t" + data[k].ToString()).ToArray());
		}

		public static string ToStringInverted<TKey, TValue>(this IDictionary<TKey, TValue> data)
		{
			return string.Join(Environment.NewLine, data.Keys.OrderByDescending(key => data[key]).Select(k => data[k].ToString() + "\t" + k.ToString()).ToArray());
		}


		public static string[] GetStemmedWords(IStemmer stemmer, string text)
		{
			var noHTMLWords = text.SplitInWordsAndStripHTML();
			var words = noHTMLWords.Select(stemmer.Stem).ToArray();
			return words;
		}

		public static SortedDictionary<TKey, double> DistributionQuotient<TKey>(SortedDictionary<TKey, int> numerator,
		                                                                  SortedDictionary<TKey, int> denominator)
		{
			var result = new SortedDictionary<TKey, double>();
			foreach (var w in numerator.Where(w => denominator.ContainsKey(w.Key)))
				result.Add(w.Key, (double) w.Value/denominator[w.Key]);
			return result;
		}
		public static void UpdateOrAdd<TKey,TValue>(this IDictionary<TKey,TValue> dictionary, TKey key, Func<TValue,TValue> func, TValue deafultValue  )
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = func(dictionary[key]);
			}
			else
			{
				dictionary.Add(key,deafultValue);
			}
		}
		public static void DetectTime(this Action a, string message)
		{
			var start = DateTime.Now;
			a.Invoke();
			Console.WriteLine("{0} in: {1}", message, (DateTime.Now - start).TotalSeconds);
		}
		public static T DetectTime<T>(this Func<T> f, string message)
		{
			var start = DateTime.Now;
			var result = f.Invoke();
			Console.WriteLine("{0} in: {1}", message, (DateTime.Now - start).TotalSeconds);
			return result;
		}

		[TestFixture]
		internal class DetectTimeTest
		{
			[Test, Explicit]
			public void TestTime()
			{
				new Func<int>(() => 1).DetectTime("trolo");
			}
		}



	}
}
