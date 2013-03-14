using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace cqa_medical.DataInput.Stemmers.MyStemmer
{
	public class MyStemmer : IStemmer
	{
		private readonly Vocabulary vocabulary;
		public MyStemmer(string uniqueSampleName, IEnumerable<string> words)
		{
			File.WriteAllLines(uniqueSampleName, words, Encoding.GetEncoding(1251));
			vocabulary = new Vocabulary(uniqueSampleName);
		}

		public MyStemmer(params string[] fileNames)
		{
			vocabulary = new Vocabulary(fileNames);
		}

		public Vocabulary GetVocabulary()
		{
			return vocabulary;
		}

		public string Stem(string word)
		{
			var wordInfo = vocabulary.FindWordInfo(word.ToLower());
			return wordInfo != null ? wordInfo.Stem : word.ToLower();
		}
		public AdditionalInfo StemWithInfo(string word)
		{
			var lword = word.ToLower();
			var wordInfo = vocabulary.FindWordInfo(lword);
			return wordInfo != null ? new AdditionalInfo(wordInfo.Stem, true) : new AdditionalInfo(lword, false);
		}
	}

	public class AdditionalInfo
	{
		public bool IsStemmed { get; private set; }
		public string StemmedWord { get; private set; }


		public string StemmedCheckedWord { get { return IsStemmed ? StemmedWord : ""; } }

		public AdditionalInfo(string stemmedWord, bool isStemmed)
		{
			IsStemmed = isStemmed;
			StemmedWord = stemmedWord;
		}
	}

	public class StemInfo
	{
		public string Stem { get; set; }
		public string PartOfSpeach { get; set; }

		public StemInfo(string stemmedWord, string partOfSpeach)
		{
			Stem = stemmedWord;
			PartOfSpeach = partOfSpeach;
		}
	}

	[TestFixture]
	internal class MyStemTest
	{
		[Test]
		public void TestDictionaryLoading()
		{
			var stemmer = new MyStemmer(Program.QuestionsFileName, Program.AnswersFileName);
			Assert.AreEqual(661255, stemmer.GetVocabulary().GetWordInfos().Count);
			Assert.AreEqual("сильный", stemmer.Stem("СилЬнЫх"));
		}

		[Test]
		public void SomeWordsTest()
		{
			var ww = new string[] {"проверка", "прибыла","прибытие", "воплощать"};
			var stemmer = new MyStemmer("TestSample.txt", ww);
			foreach (var s1 in ww)
			{
				Console.Out.WriteLine(stemmer.Stem(s1)); 
				
			}
			Assert.Fail();

		}
	}
}
