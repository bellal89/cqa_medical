using NUnit.Framework;

namespace cqa_medical.DataInput.Stemmers.MyStemmer
{
	class MyStemmer : IStemmer
	{
		private readonly Vocabulary vocabulary;

		public MyStemmer(Vocabulary vocabulary)
		{
			this.vocabulary = vocabulary;
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

	internal class AdditionalInfo
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
			var vocabulary = new Vocabulary(Program.QuestionsFileName, Program.AnswersFileName);
			var stemmer = new MyStemmer(vocabulary);
			Assert.AreEqual(661255, vocabulary.GetWordInfos().Count);
			Assert.AreEqual("сильный", stemmer.Stem("СилЬнЫх"));
		}
	}
}
