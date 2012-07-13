using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace cqa_medical.DataInput.MyStem
{

	class MyStemUnit
	{
		internal class MyStemWord
		{
			public string Word;
			public bool IsReliable;
			public MyStemWord(string word, bool isReliable)
			{
				Word = word;
				IsReliable = isReliable;
			}

		}

		public List<MyStemWord> Words;
		public String Word
		{
			get
			{
				foreach (var s in Words)
				{
					if (s.IsReliable)
						return s.Word;
				}
				return Words[0].Word;
			}
		}

		public IEnumerable<string> GetAllWords()
		{
			return Words.Select(t => t.Word);
		}

		public MyStemUnit(string stringInMyStemFormat)
		{
			Words = new List<MyStemWord>();
			var wordVariants = stringInMyStemFormat.Split('|');
			foreach(var s in wordVariants)
			{
				if (s[s.Length-1] == '?') // присутствие ? в конце строки значит, что MyStem не знает этого слова и сгенерировал его алгоритмами
				{
					Words.Add(new MyStemWord(s.Substring(0, s.Length - 1), false));
				}
				else
				{
					Words.Add(new MyStemWord(s, true));
				}
			}

		}

		public override string ToString()
		{
			return Word;
		}
	}

	[TestFixture]
	internal class MyStemUnitTest
	{
		//[Test]
		//public void 

	}

}
