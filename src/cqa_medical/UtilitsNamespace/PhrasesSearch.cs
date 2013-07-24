using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.UtilitsNamespace
{
	class PhrasesSearch
	{
		private readonly List<HashSet<String>>  searchFields;

		public PhrasesSearch(string searchIn)
		{
			searchFields = new List<HashSet<string>>();
			foreach (var s in searchIn.SplitIntoSentences())
			{
				searchFields.Add(new HashSet<string>(s.SplitIntoWords()));
			}
		}

		public bool Contains(Phrase s)
		{
			return Contains(s.WordsInPhrase);
		}
		private bool Contains(IEnumerable<string> requiredStrings)
		{
			return searchFields.Any(f => requiredStrings.All(f.Contains));
		}
		
	}

	internal class Phrase
	{
		public HashSet<string> WordsInPhrase;

		public Phrase(HashSet<string> wordsInPhrase)
		{
			WordsInPhrase = wordsInPhrase;
		}

		public Phrase(string phrase)
			:this (new HashSet<string>(phrase.SplitIntoWords()))
		{
		}
		public Phrase(string phrase, IStemmer stemmer)
			:this (new HashSet<string>(phrase.SplitIntoWordsAndStem(stemmer)))
		{
		}
	}

	[TestFixture]
	class PhrasesSearchTest
	{
		[Test]
		public void qwe()
		{
			var sq = new Phrase("грипп орви", Program.DefaultMyStemmer);
			var ql = Program.DefaultQuestionList;
			using (var writer = new StreamWriter("texts.txt"))
			{
				foreach (var text in ql.GetAllQuestions().Select(q => q.WholeText))
				{
					var q = new PhrasesSearch(text);
					if (q.Contains(sq))
					{
						writer.WriteLine(text);
					}
				}
			}
		}

	}
}
