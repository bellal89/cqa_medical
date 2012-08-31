﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.UtilitsNamespace
{
	internal static class Utilits
	{
		#region StringModify

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

		#endregion


		#region FormatString

		public static string ToStringNormal<TKey, TValue>(this IDictionary<TKey, TValue> data, string delimiter = "\t")
		{
			return string.Join(Environment.NewLine, data.Keys.Select(k => k.ToString() + delimiter + data[k].ToString()));
		}
		public static string ToStringSortedByValue<TKey, TValue>(this IDictionary<TKey, TValue> data, string delimiter = "\t")
		{
			return string.Join(Environment.NewLine, data.Keys.OrderByDescending(k => data[k]).Select(k => k.ToString() + delimiter + data[k].ToString()));
		}

		public static string ToStringInverted<TKey, TValue>(this IDictionary<TKey, TValue> data, string delimiter = "\t")
		{
			return string.Join(Environment.NewLine,
			                   data.Keys.OrderByDescending(key => data[key]).Select(
			                   	k => data[k].ToString() + delimiter + k.ToString()));
		}

		public static string ToStringComparable<TValue>(this IDictionary<DateTime, TValue> data, string delimiter = "\t")
		{
			return string.Join(Environment.NewLine,
			                   data.Keys.Select(k => k.ToString("yyyy-MM-dd") + delimiter + data[k].ToString()));
		}

		public static string EmptyOrFormat(this string s, string format)
		{
			if (string.IsNullOrEmpty(s)) return "";
			return String.Format(format, s);
		}

		#region HeadTailStructureFormat

		public static string FormatString<THead, TValues>(this KeyValuePair<THead, IEnumerable<TValues>> headListPair,
		                                                  string delimiter = "\t", string valuesDelimiter = ", ")
		{
			return headListPair.Key + delimiter + String.Join(valuesDelimiter, headListPair.Value);
		}
		public static string FormatString<THead, TValues>(this Tuple<THead, IEnumerable<TValues>> headListPair,
		                                                  string delimiter = "\t", string valuesDelimiter = ", ")
		{
			return headListPair.Item1 + delimiter + String.Join(valuesDelimiter, headListPair.Item2);
		}

		public static string FormatString<THead, TValues>(this KeyValuePair<THead, HashSet<TValues>> headListPair,
		                                                  string delimiter = "\t", string valuesDelimiter = ", ")
		{
			return headListPair.Key + delimiter + String.Join(valuesDelimiter, headListPair.Value);
		}

		#endregion
		
		
		#endregion

		#region DictionaryDataWork

		public static TValue GetOrDefault<TKey, TValue>(this SortedDictionary<TKey, TValue> dict, TKey key,
		                                                TValue defaultValue)
		{
			return dict.ContainsKey(key) ? dict[key] : defaultValue;
		}

		public static void UpdateOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
		                                             Func<TValue, TValue> func, TValue deafultValue)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = func(dictionary[key]);
			}
			else
			{
				dictionary.Add(key, deafultValue);
			}
		}

		public static SortedDictionary<TKey, double> DistributionQuotient<TKey>(SortedDictionary<TKey, int> numerator,
		                                                                        SortedDictionary<TKey, int> denominator)
		{
			var result = new SortedDictionary<TKey, double>();
			foreach (var w in numerator.Where(w => denominator.ContainsKey(w.Key)))
				result.Add(w.Key, (double) w.Value/denominator[w.Key]);
			return result;
		}

		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs)
		{
			return pairs.ToDictionary(q => q.Key, q => q.Value);
		}

		#endregion



		public static string[] GetStemmedWords(IStemmer stemmer, string text)
		{
			var wordsWithoutHTML = text.SplitInWordsAndStripHTML();
			var words = wordsWithoutHTML.Select(stemmer.Stem).ToArray();
			return words;
		}

	
		public static byte[] ReadAllBytes(this Stream source)
		{
			var answer = new List<byte>();

			while (true)
			{
				int currentByte = source.ReadByte();
				if (currentByte == -1) break;
				answer.Add((byte)currentByte);
			}
			return answer.ToArray();
		}

		#region TimeDetect

		public static void DetectTime(this Action a, string message)
		{
			var start = DateTime.Now;
			var begin = Process.GetCurrentProcess().TotalProcessorTime;
			a.Invoke();
			var cpuResult = (Process.GetCurrentProcess().TotalProcessorTime - begin).TotalSeconds;
			var datetimeResult = (DateTime.Now - start).TotalSeconds;
			Console.WriteLine("{0} in {1} seconds. CPU Time - {2} seconds", message, datetimeResult, cpuResult);
		}

		public static T DetectTime<T>(this Func<T> f, string message)
		{
			var start = DateTime.Now;
			var begin = Process.GetCurrentProcess().TotalProcessorTime;
			var result = f.Invoke();
			var cpuResult = (Process.GetCurrentProcess().TotalProcessorTime - begin).TotalSeconds;
			var datetimeResult = (DateTime.Now - start).TotalSeconds;
			Console.WriteLine("{0} in {1} seconds. CPU Time - {2} seconds", message, datetimeResult, cpuResult);
			return result;
		}

		#endregion

		#region ShrinkDate


		public static DateTime GetWeek(this DateTime now)
		{
			return now.AddDays(-(int) now.DayOfWeek);
		}

		public static SortedDictionary<DateTime, int> SumUpToDays(this IDictionary<DateTime, int> dictionary)
		{
			var result = new SortedDictionary<DateTime, int>();
			foreach (var key in dictionary.Keys)
			{
				var q = new DateTime(key.Year, key.Month, key.Day);
				result.UpdateOrAdd(q, v => v + dictionary[key], dictionary[key]);
			}
			return result;
		}

		public static SortedDictionary<DateTime, int> SumUpToWeeks(this IDictionary<DateTime, int> dictionary)
		{
			var result = new SortedDictionary<DateTime, int>();
			foreach (var key in dictionary.Keys)
			{
				var q = GetWeek(key);
				q = new DateTime(q.Year, q.Month, q.Day);
				result.UpdateOrAdd(q, v => v + dictionary[key], dictionary[key]);
			}
			return result;
		}

		public static SortedDictionary<DateTime, double> SumUpToDays(this IDictionary<DateTime, double> dictionary)
		{
			var result = new SortedDictionary<DateTime, double>();
			foreach (var key in dictionary.Keys)
			{
				var q = new DateTime(key.Year, key.Month, key.Day);
				result.UpdateOrAdd(q, v => v + dictionary[key], dictionary[key]);
			}
			return result;
		}

		public static SortedDictionary<DateTime, double> SumUpToWeeks(this IDictionary<DateTime, double> dictionary)
		{
			var result = new SortedDictionary<DateTime, double>();
			foreach (var key in dictionary.Keys)
			{
				var q = GetWeek(key);
				q = new DateTime(q.Year, q.Month, q.Day);
				result.UpdateOrAdd(q, v => v + dictionary[key], dictionary[key]);
			}
			return result;
		}

		#endregion


	}
	[TestFixture]
	internal class DetectTimeTest
	{
		[Test]
		public void TestTime()
		{
			Console.WriteLine(new Func<long>(
				() =>
				{
					long q = 0;
					for (int i = 0; i < 50590000; i++)
					{
						q += i;
					}
					return q;
				}
				).DetectTime("trolo"));
		}

		[Test]
		public void TestSumUp()
		{
			var q = new SortedDictionary<DateTime, int>();
			var dateTime = new DateTime(2, 1, 1);
			q[dateTime.AddHours(1)] = 5;
			q[dateTime] = 6;
			var w = q.SumUpToWeeks();
			Assert.AreEqual(11, w[dateTime.GetWeek()]);

		}

	}

}
