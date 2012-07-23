using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace cqa_medical.DataInput.Stemmers.MyStemmer
{
	class Vocabulary
	{
		private readonly Dictionary<string, StemInfo> wordToWordInfo = new Dictionary<string, StemInfo>();
		private readonly string[] fileNames;

		public Vocabulary(params string[] fileNames)
		{
			this.fileNames = fileNames;
		}

		public void InitDictionary()
		{
			foreach (var parts in fileNames
				.Select(File.ReadAllLines)
				.SelectMany(lines => lines
					.Select(line => line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries))
					.Where(parts => parts.Length > 2 && !wordToWordInfo.ContainsKey(parts[0]))))
			{
				wordToWordInfo.Add(parts[0], new StemInfo(parts[1], parts[2]));
			}
		}
		public IEnumerable<string> GetPartOfSpeech(string partOfSpeech, string text)
		{
			return text.SplitIntoWords().Select(GetPartOfSpeech).Where(p => p == partOfSpeech);
		}

		public string GetPartOfSpeech(string word)
		{
			return wordToWordInfo.ContainsKey(word) ? wordToWordInfo[word].PartOfSpeach : null;
		}

		public Dictionary<string, StemInfo> GetWordInfos()
		{
			return wordToWordInfo;
		}

		public StemInfo FindWordInfo(string word)
		{
			StemInfo wordInfo;
			if (wordToWordInfo.TryGetValue(word, out wordInfo)) return wordInfo;
			return null;
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
