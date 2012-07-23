using System;
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
			Console.WriteLine(piParadigmCollection[0].Norm);
			Console.WriteLine(piParadigmCollection[0].SrcNorm);
			Console.WriteLine(piParadigmCollection[0].SrcAncode);
			Console.WriteLine(piParadigmCollection[0].TypeAncode);
			Console.WriteLine(piParadigmCollection[0].Ancode[0]);
			return piParadigmCollection[0].Norm;
		}
	}
	
	[TestFixture]
	public class AOTLemmatizer_Test
	{
		[Test]
		public void Test()
		{
			var lemmatizer = new AOTLemmatizer();
			lemmatizer.Stem("дороге");
		}
	}

}
