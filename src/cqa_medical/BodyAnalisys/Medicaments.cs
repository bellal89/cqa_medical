using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.DataInput.Stemmers.MyStemmer;

namespace cqa_medical.BodyAnalisys
{
	class Medicaments
	{
		private readonly Dictionary<string, HashSet<int>> invertedIndex = new Dictionary<string, HashSet<int>>();
		private readonly IStemmer stemmer;
		
		public Medicaments(IStemmer stemmer, string medicamentsFileName)
		{
			this.stemmer = stemmer;
			invertedIndex = LoadIndexFromFile(medicamentsFileName);
		}

		private Dictionary<string, HashSet<int>> LoadIndexFromFile(string fileName)
		{
			var index = new Dictionary<string, HashSet<int>>();
			var i = 1;
			foreach (var parts in File.ReadAllLines(fileName)
									  .Distinct()
									  .Select(line => line.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries))
									  .Where(item => item.Length > 1))
			{
				AddToIndex(index, parts[0], i);
				AddToIndex(index, parts[1], i);
				i++;
			}
			return Filter(index);
		}

		private static Dictionary<string, HashSet<int>> Filter(Dictionary<string, HashSet<int>> index)
		{
			return index.Where(item => item.Value.Count <= 85 && item.Key.Length > 2).ToDictionary(item => item.Key, item => item.Value);
		}

		private void AddToIndex(IDictionary<string, HashSet<int>> index, string medicamentFullName, int medicamentId)
		{
			var words = medicamentFullName.ToLower().Split(new[] { ' ', '®', '+', '-', '—', '~', '[', ']', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
			
			if (words.Length == 0) return;
			
			var stemmedWord = stemmer.Stem(words[0]);
			if (!index.ContainsKey(stemmedWord))
			{
				index.Add(stemmedWord, new HashSet<int>());
			}
			index[stemmedWord].Add(medicamentId);
		}

		public override string ToString()
		{
			return String.Join("\n",
			            invertedIndex.OrderByDescending(item => item.Value.Count).Select(
			            	item => item.Value.Count + "\t" + item.Key + "\t" + String.Join(" ", item.Value.OrderBy(id => id))));
		}

		public IEnumerable<string> GetMedicamentNames()
		{
			return invertedIndex.Keys;
		}
	}

	[TestFixture]
	public class MedicamentsTest
	{
		[Test]
		public void CreationTest()
		{
			var medicaments = new Medicaments(
				new MyStemmer(new Vocabulary(Program.QuestionsFileName, Program.AnswersFileName)),
				Program.MedicamentsFileName);
			Console.WriteLine(medicaments);
			File.WriteAllText("MedOutput2.txt", medicaments.ToString());
		}
	}
}
