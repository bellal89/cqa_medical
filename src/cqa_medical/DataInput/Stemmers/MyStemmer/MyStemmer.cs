using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace cqa_medical.DataInput.Stemmers.MyStemmer
{
	class Vocabulary
	{
		private readonly Dictionary<string, StemInfo> wordToStemInfo = new Dictionary<string, StemInfo>();
		private readonly string[] fileNames;

		public Vocabulary(params string[] fileNames)
		{
			this.fileNames = fileNames;
		}

		public IEnumerable<string> GetPartOfSpeech(string partOfSpeech, string text)
		{
			return text.SplitIntoWords().Select(GetPartOfSpeech).Where(p => p == partOfSpeech);
		}

		public string GetPartOfSpeech(string word)
		{
			return wordToStemInfo.ContainsKey(word) ? wordToStemInfo[word].PartOfSpeach : null;
		}

		public Dictionary<string, StemInfo> GetWordInfos()
		{
			return wordToStemInfo;
		}

		public StemInfo FindWordInfo(string word)
		{
			StemInfo wordInfo;
			return wordToStemInfo.TryGetValue(word, out wordInfo) ? wordInfo : null;
		}
	}
	
	class MyStemmer : IStemmer
	{
		private readonly Vocabulary vocabulary;

		public MyStemmer(Vocabulary vocabulary)
		{
			this.vocabulary = vocabulary;
		}

		public string Stem(string word)
		{
			var wordInfo = vocabulary.FindWordInfo(word);
			return wordInfo != null ? wordInfo.Stem : word.ToLower();
		}
	}

	internal class StemInfo
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
	public class MyStemTest
	{
		[Test]
		public void TestDictionaryLoading()
		{
			var vocabulary = new Vocabulary("../../Files/qst_stemmed.txt", "../../Files/ans_stemmed2.txt");
			var stemmer = new MyStemmer(vocabulary);
			Console.WriteLine(vocabulary.GetWordInfos().Count);
		}
	}
}
