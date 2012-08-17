using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.UtilitsNamespace
{
	internal static class Utilits
	{
		public static DateTime GetWeek(DateTime now)
		{
			return now.AddDays(-(int)now.DayOfWeek);
		}
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

		public static string ToStringNormal<TKey, TValue>(this IDictionary<TKey, TValue> data, string delimiter = "\t")
		{
			return string.Join(Environment.NewLine, data.Keys.Select(k => k.ToString() + delimiter + data[k].ToString()));
		}

		public static string ToStringInverted<TKey, TValue>(this IDictionary<TKey, TValue> data, string delimiter = "\t")
		{
			return string.Join(Environment.NewLine, data.Keys.OrderByDescending(key => data[key]).Select(k => data[k].ToString() + delimiter + k.ToString()));
		}
		public static string ToStringComparable<TValue>(this IDictionary<DateTime, TValue> data, string delimiter = "\t")
		{
			return string.Join(Environment.NewLine, data.Keys.Select(k => k.ToString("yyyy-MM-dd") + delimiter + data[k].ToString()));
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
			Console.WriteLine("{0} in {1} seconds", message, (DateTime.Now - start).TotalSeconds);
		}
		public static T DetectTime<T>(this Func<T> f, string message)
		{
			var start = DateTime.Now;
			var result = f.Invoke();
			Console.WriteLine("{0} in {1}", message, (DateTime.Now - start).TotalSeconds);
			return result;
		}

		public static SortedDictionary<DateTime, int> SumUpToDays(this IDictionary<DateTime, int> dictionary)
		{
			var result = new SortedDictionary<DateTime, int>();
			foreach (var key in dictionary.Keys)
			{
				var q = new DateTime(key.Year,key.Month,key.Day);
				result.UpdateOrAdd(q, v => v + dictionary[key], dictionary[key]);
			}
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
			[Test, Explicit]
			public void TestSumUp()
			{
				var q = new SortedDictionary<DateTime, int>();
				var dateTime = new DateTime(1, 1, 1);
				q[dateTime.AddHours(1)] = 5;
				q[dateTime] = 6;
				var w = SumUpToDays(q);
				Assert.AreEqual(11, w[dateTime]);

			}
		}



	}
}
