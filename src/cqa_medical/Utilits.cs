using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace cqa_medical
{
	static class  Utilits
	{
		public static string StripHTMLTags(this String s)
		{
			return Regex.Replace(s, "<[^>]*?>", string.Empty, RegexOptions.IgnoreCase);
		}
		public static IEnumerable<string> SplitInWords(this string s)
		{
			return Regex.Split(s, @"\W+");
		}
	}
}
