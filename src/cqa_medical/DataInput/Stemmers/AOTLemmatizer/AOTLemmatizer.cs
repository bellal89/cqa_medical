using System.Collections.Generic;
using System.Linq;
using LEMMATIZERLib;
using NUnit.Framework;

namespace cqa_medical.DataInput.Stemmers.AOTLemmatizer
{
	class AOTLemmatizer : IStemmer
	{
		private readonly ILemmatizer lemmatizerRu;
		/// <summary>
		/// this needs 32 bit version of project
		/// You can change it in project properties
		/// </summary>
		public AOTLemmatizer()
		{
			lemmatizerRu = new LemmatizerRussian();
			lemmatizerRu.LoadDictionariesRegistry();
		}

		public string Stem(string word)
		{
			return GetAllNorms(word).First();
		}
		public IEnumerable<string> GetAllNorms(string word)
		{
			IParadigmCollection piParadigmCollection = lemmatizerRu.CreateParadigmCollectionFromForm(word, 0, 0);
			var len = piParadigmCollection.Count;
			for (int i = 0; i < len; ++i )
				yield return piParadigmCollection[i].Norm.ToLower();
		}
		public IEnumerable<string> GetAllForms(string word)
		{
			IParadigmCollection piParadigmCollection = lemmatizerRu.CreateParadigmCollectionFromNorm(word, 0, 0);

			for (int i = 0; i < piParadigmCollection.Count; ++i)
			{
				for (uint j = 0; j < piParadigmCollection[i].Count; j++)
				{
					yield return piParadigmCollection[i][j].ToLower();
				}
			}
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
