using System.Collections.Generic;
using System.IO;
using System.Linq;
using cqa_medical.DataInput;

namespace cqa_medical.UtilitsNamespace.Parsers
{
	abstract class HTMLPageParser<T> where T : class
	{
		private readonly string pagesDirectory;
		private readonly string directoryForSerialization;

		protected HTMLPageParser(string pagesDirectory)
		{
			this.pagesDirectory = pagesDirectory;
			directoryForSerialization = pagesDirectory + "serialized/";
		}

		public IEnumerable<T> ParsePages()
		{
			return Directory.GetFiles(pagesDirectory).Select(ParsePage).Where(p => p != null);
		}
		
		public IEnumerable<T> GetObjects()
		{
			if (!Directory.Exists(directoryForSerialization) || Directory.GetFiles(directoryForSerialization).Length == 0)
				SerializeObjects();
			
			return Directory.GetFiles(directoryForSerialization).Select(ObjectSerializer.GetFromFile<T>).Where(u => u != null).ToList();
		}

		public void SerializeObjects()
		{
			if (!Directory.Exists(directoryForSerialization))
				Directory.CreateDirectory(directoryForSerialization);

			foreach (var file in Directory.GetFiles(pagesDirectory))
			{
				var page = ParsePage(file);
				if (page != null)
				{
					ObjectSerializer.SaveToFile(page, directoryForSerialization + Path.GetFileName(file) + ".serialized");
				}
			}
		}

		protected abstract T ParsePage(string fileName);
	}
}
