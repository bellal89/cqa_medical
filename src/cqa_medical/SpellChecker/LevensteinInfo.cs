using System;
using System.Collections.Generic;

namespace cqa_medical.SpellChecker
{
	internal class LevensteinInfo
	{
		private readonly string word;
		private readonly string dictionaryWord;
		private readonly int[,] matrix;

		public LevensteinInfo(string dictionaryWord, string word)
		{
			this.word = word;
			this.dictionaryWord = dictionaryWord;
			matrix = CalculateLevensteinMatrix(word, dictionaryWord);
		}

		public string GetWord()
		{
			return word;
		}

		public string GetDictionaryWord()
		{
			return dictionaryWord;
		}

		public int GetDistance()
		{
			return matrix[word.Length, dictionaryWord.Length];
		}

		private static int[,] CalculateLevensteinMatrix(string s1, string s2)
		{
			if (s1 == null) throw new ArgumentNullException("s1");
			if (s2 == null) throw new ArgumentNullException("s2");
			var m = new int[s1.Length + 1, s2.Length + 1];

			for (int i = 0; i <= s1.Length; i++) m[i, 0] = i;
			for (int j = 0; j <= s2.Length; j++) m[0, j] = j;

			for (int i = 1; i <= s1.Length; i++)
				for (int j = 1; j <= s2.Length; j++)
				{
					int diff = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

					var del = m[i - 1, j] + 1; // deletion
					var ins = m[i, j - 1] + 1; // insertion
					var subst = m[i - 1, j - 1] + diff; // substitution

					m[i, j] = Math.Min(Math.Min(del, ins), subst);

					// transition
					if (i > 1 && j > 1 && s1[i - 1] == s2[j - 2] && s1[i - 2] == s2[j - 1])
					{
						m[i, j] = Math.Min(
							m[i, j],
							m[i - 2, j - 2] + diff
							);
					}
				}
			return m;
		}

		public Tuple<string, string> GetMisspelling()
		{
			var minLen = Math.Min(dictionaryWord.Length, word.Length);
			var i = 0;
			while (i < minLen && dictionaryWord[i] == word[i])
			{
				i++;
			}
			if (i == minLen)
			{
				string s1 = "", s2 = "";
				if (dictionaryWord.Length > word.Length)
					s1 = "" + dictionaryWord[i];
				else if (dictionaryWord.Length < word.Length)
					s2 = "" + word[i];
				return Tuple.Create(s1, s2);
			}
			if (i + 1 < dictionaryWord.Length && i + 1 < word.Length && dictionaryWord[i+1] == word[i] && dictionaryWord[i] == word[i+1])
			{
				return Tuple.Create(("" + dictionaryWord[i]) + dictionaryWord[i + 1], ("" + word[i]) + word[i + 1]);
			}
			return Tuple.Create("" + dictionaryWord[i], "" + word[i]);
		}
	}
}