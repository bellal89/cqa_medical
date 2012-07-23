using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace cqa_medical.DataInput.Stemmers.MyStemmer
{
	class MyStemmer : IStemmer
	{
		private readonly string[] fileNames;
		private readonly Dictionary<string, MystemWordInfo> wordInfos = new Dictionary<string, MystemWordInfo>();

		public MyStemmer(params string[] fileNames)
		{
			this.fileNames = fileNames;
		}

		public void InitDictionary()
		{
			foreach (var parts in fileNames
				.Select(File.ReadAllLines)
				.SelectMany(lines => lines
					.Select(line => line.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries))
					.Where(parts => parts.Length > 2 && !wordInfos.ContainsKey(parts[0]))))
			{
				wordInfos.Add(parts[0], new MystemWordInfo(parts[1], parts[2]));
			}
		}

		public string Stem(string s)
		{
			return String.Join(" ", s.SplitIntoWords().Select(w => wordInfos.ContainsKey(w) ? wordInfos[w].Word : w.ToLower()));
		}

		public IEnumerable<string> GetPartOfSpeach (string partOfSpeach, string s)
		{
			return s.SplitIntoWords().Select(GetPartOfSpeach).Where(p => p == partOfSpeach);
		}

		public string GetPartOfSpeach(string word)
		{
			return wordInfos.ContainsKey(word) ? wordInfos[word].PartOfSpeach : null;
		}

		public Dictionary<string, MystemWordInfo> GetWordInfos()
		{
			return wordInfos;
		}
	}

	internal class MystemWordInfo
	{
		public string Word { get; set; }
		public string PartOfSpeach { get; set; }

		public MystemWordInfo(string stemmedWord, string partOfSpeach)
		{
			Word = stemmedWord;
			PartOfSpeach = partOfSpeach;
		}
	}

	[TestFixture]
	public class MyStemTest
	{
		[Test]
		public void TestDictionaryLoading()
		{
			var stemmer = new MyStemmer("../../Files/qst_stemmed.txt", "../../Files/ans_stemmed2.txt");
			stemmer.InitDictionary();
			Console.WriteLine(stemmer.GetWordInfos().Count);
		}
	}
}
