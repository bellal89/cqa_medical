using System;
using System.Collections.Generic;

namespace cqa_medical.SpellChecker
{
	internal class LevensteinInfo
	{
		private readonly string word;
		private readonly string dictionatyWord;
		private readonly int[,] matrix;

		public LevensteinInfo(string dictionatyWord, string word)
		{
			this.word = word;
			this.dictionatyWord = dictionatyWord;
			matrix = CalculateLevensteinMatrix(word, dictionatyWord);
		}

		public string GetWord()
		{
			return word;
		}

		public string GetDictionaryWord()
		{
			return dictionatyWord;
		}

		public int GetDistance()
		{
			return matrix[word.Length, dictionatyWord.Length];
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

		public List<Tuple<string, string>> GetMisspellings()
		{
			var misspellings = new List<Tuple<string, string>>();
			int i = word.Length, j = dictionatyWord.Length;
			while (i != 0 || j != 0)
			{
				if (i > 0 && j > 0 && word[i - 1] == dictionatyWord[j - 1])
				{
					i--;
					j--;
				}
				else
				{
					var prevPosList = GetPreviousMinPositions(i, j);
					if (prevPosList.Count == 0)
						throw new Exception("Levenstein: Previous minimal positions not found!");
					if (prevPosList.Count > 1)
					{
						if (i < 1 || j < 1)
							throw new Exception("Levenstein: Transition found but we are on the 1st symbol.");
						if (i == 1 || j == 1)
						{
							i--;
							j--;
							misspellings.Add(Tuple.Create("" + word[i], "" + dictionatyWord[j]));
							break;
						}
						i -= 2;
						j -= 2;
						misspellings.Add(Tuple.Create("" + word[i] + word[i + 1], "" + dictionatyWord[j] + dictionatyWord[j + 1]));
						if (word[i] != dictionatyWord[j + 1])
							misspellings.Add(Tuple.Create("" + word[i], "" + dictionatyWord[j + 1]));
						if (word[i + 1] != dictionatyWord[j])
							misspellings.Add(Tuple.Create("" + word[i + 1], "" + dictionatyWord[j]));
						break;
					}
					if (prevPosList[0].Item1 == i)
					{
						j--;
						misspellings.Add(Tuple.Create("", "" + dictionatyWord[j]));
						break;
					}
					if (prevPosList[0].Item2 == j)
					{
						i--;
						misspellings.Add(Tuple.Create("" + word[i], ""));
						break;
					}
					i--;
					j--;
					misspellings.Add(Tuple.Create("" + word[i], "" + dictionatyWord[j]));
				}
			}
			return misspellings;
		}

		private List<Tuple<int, int>> GetPreviousMinPositions(int i, int j)
		{
			if (i == 0 && j == 0)
				return new List<Tuple<int, int>> { Tuple.Create(i, j) };
			if (i == 0)
				return new List<Tuple<int, int>> { Tuple.Create(i, j - 1) };
			if (j == 0)
				return new List<Tuple<int, int>> { Tuple.Create(i - 1, j) };

			var prevPosList = new List<Tuple<int, int>>();
			var prevMin = Math.Min(matrix[i - 1, j - 1], Math.Min(matrix[i - 1, j], matrix[i, j - 1]));
			if (matrix[i - 1, j - 1] == prevMin)
				prevPosList.Add(Tuple.Create(i - 1, j - 1));
			if (matrix[i - 1, j] == prevMin)
				prevPosList.Add(Tuple.Create(i - 1, j));
			if (matrix[i, j - 1] == prevMin)
				prevPosList.Add(Tuple.Create(i, j - 1));
			return prevPosList;
		}
	}
}