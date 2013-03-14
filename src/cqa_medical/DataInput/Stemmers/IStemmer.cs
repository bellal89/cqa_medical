using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers.MyStemmer;

namespace cqa_medical.DataInput.Stemmers
{
    public interface IStemmer
    {
		string Stem(string word);
    }


	[TestFixture]
	internal class StemmersComparisonTest
	{
		[Test, Explicit]
		public void StemmersComparison()
		{
			SaveMyStemWords();
		}

		private static void SaveMyStemWords()
		{
			var vocabulary = new Vocabulary(Program.QuestionsFileName, Program.AnswersFileName);
			var stemToWord = new Dictionary<string, List<string>>();
			foreach (var stemInfo in vocabulary.GetWordInfos())
			{
				if (stemToWord.ContainsKey(stemInfo.Value.Stem))
				{
					stemToWord[stemInfo.Value.Stem].Add(stemInfo.Key);
				}
				else
				{
					stemToWord.Add(stemInfo.Value.Stem, new List<string>());
				}
			}
			File.WriteAllText("MyStem.txt", String.Join("\n", stemToWord.Select(kv => String.Join(" ", kv.Value))));
		}
	}
}
