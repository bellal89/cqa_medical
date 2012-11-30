using System.Collections.Generic;
using System.IO;
using System.Linq;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;

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

		protected string GetPagesDirectory()
		{
			return pagesDirectory;
		}

		public IEnumerable<T> ParsePages()
		{
			return Directory.GetFiles(pagesDirectory).Select(ParsePage).Where(p => p != null);
		}
		
		public IEnumerable<T> GetObjects()
		{
			if (!IsObjectsSerialized())
				SerializeObjects();
			
			return Directory.GetFiles(directoryForSerialization).Select(ObjectSerializer.GetFromFile<T>).Where(u => u != null).ToList();
		}

		protected bool IsObjectsSerialized()
		{
			return Directory.Exists(directoryForSerialization) && Directory.GetFiles(directoryForSerialization).Length != 0;
		}

		protected void CleanSerializedObjects()
		{
			if (!IsObjectsSerialized()) return;
			foreach (var fileName in Directory.GetFiles(directoryForSerialization))
			{
				File.Delete(fileName);
			}
		}

		private void SerializeObjects()
		{
			if (!Directory.Exists(directoryForSerialization))
				Directory.CreateDirectory(directoryForSerialization);

			foreach (var file in Directory.GetFiles(pagesDirectory).Where(fileName => !fileName.EndsWith(MyStemLoader.GetStemmedFileSuffix())))
			{
				var page = ParsePage(file);
				if (page != null && IsUnique(page))
				{
					ObjectSerializer.SaveToFile(page, directoryForSerialization + Path.GetFileName(file) + ".serialized");
				}
			}
		}

		protected abstract bool IsUnique(T page);

		protected abstract T ParsePage(string fileName);
	}
}
