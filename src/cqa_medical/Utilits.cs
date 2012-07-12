using System;
using System.Text.RegularExpressions;

namespace cqa_medical
{
	static class  Utilits
	{
		public static string StripHTMLTags(this String s)
		{
			return Regex.Replace(s, "<[^>]*?>", string.Empty, RegexOptions.IgnoreCase);
		}
	}
}
