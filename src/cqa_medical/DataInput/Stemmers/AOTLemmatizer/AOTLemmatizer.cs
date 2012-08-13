using NUnit.Framework;

namespace cqa_medical.DataInput.Stemmers.AOTLemmatizer
{
	class AOTLemmatizer : IStemmer
	{
		private readonly LEMMATIZERLib.ILemmatizer lemmatizerRu;
		/// <summary>
		/// this needs 32 bit version of project
		/// You can change it in project properties
		/// </summary>
		public AOTLemmatizer()
		{
			lemmatizerRu = new LEMMATIZERLib.LemmatizerRussian();
			lemmatizerRu.LoadDictionariesRegistry();
		}

		public string Stem(string word)
		{
			LEMMATIZERLib.IParadigmCollection piParadigmCollection = lemmatizerRu.CreateParadigmCollectionFromForm(word, 1, 1);
			if (piParadigmCollection.Count == 0) 
				return word;
			return piParadigmCollection[0].Norm.ToLower();
		}
	}
	
	[TestFixture]
	public class AOTLemmatizerTest
	{
		[Test]
		public void Test()
		{
			var lemmatizer = new AOTLemmatizer();
			Assert.AreEqual("дорога", lemmatizer.Stem("дороге"));
		}
	}

}
