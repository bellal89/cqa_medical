using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.DataInput.AOTLemmatizer
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

		public string Stem(string s)
		{
			LEMMATIZERLib.IParadigmCollection piParadigmCollection = lemmatizerRu.CreateParadigmCollectionFromForm(s, 0, 0);
			if (piParadigmCollection.Count == 0) 
				return s;
			return piParadigmCollection[0].Norm;
		}



	}
}
