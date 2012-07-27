using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.Utilits;

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

		public List<InvertedIndexUnit> FindMedicamentsInTexts(IEnumerable<Tuple<long, string>> idAndTextList)
		{
			var medicamentToIds = new Dictionary<string, HashSet<long>>();
			foreach (var idAndText in idAndTextList)
			{
				var words = idAndText.Item2.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
				foreach (var word in words.Where(word => invertedIndex.ContainsKey(word)))
				{
					if (!medicamentToIds.ContainsKey(word))
						medicamentToIds.Add(word, new HashSet<long>());
					medicamentToIds[word].Add(idAndText.Item1);
				}
			}
			return medicamentToIds.Select(item => new InvertedIndexUnit(item.Key, item.Value)).ToList();
		}
		public static IEnumerable<InvertedIndexUnit> GetDefault()
		{
			if (Utilits.Utilits.IsFileActual(Program.MedicamentsIndexFileName, Program.MedicamentsFileName))
			{
				var rawStrings = File.ReadAllLines(Program.MedicamentsIndexFileName);
				return rawStrings.Select(s => new InvertedIndexUnit(s));
			}

			var questionList = Program.DefaultQuestionList;

			var medicaments = new Medicaments(Program.DefaultMyStemmer, Program.MedicamentsFileName);

			var meds = medicaments
				.FindMedicamentsInTexts(questionList
						.GetAllAnswers()
						.Select(a => Tuple.Create(a.QuestionId, a.Text)))
				.ToArray();
			File.WriteAllLines(Program.MedicamentsIndexFileName, meds.Select(s => s.ToString()));
			return meds;
			
		}
	}

	[TestFixture]
	public class MedicamentsTest
	{
		[Test]
		public void CreationTest()
		{
			var questionList = Program.DefaultQuestionList;

			var medicaments = new Medicaments(Program.DefaultMyStemmer,Program.MedicamentsFileName);

			var meds =
				medicaments.FindMedicamentsInTexts(questionList.GetAllAnswers().Select(a => Tuple.Create(a.QuestionId, a.Text)));

			Console.WriteLine(medicaments);
			File.WriteAllText("MedOutput2.txt", medicaments.ToString());
		}

		[Test]
		public void IndexTest()
		{
			var questionList = Program.TestDefaultQuestionList;

			var medicaments = new Medicaments(Program.DefaultMyStemmer, Program.MedicamentsFileName);

			var meds =
				medicaments.FindMedicamentsInTexts(questionList.GetAllAnswers().Select(a => Tuple.Create(a.QuestionId, a.Text))).ToList();
			Console.WriteLine(String.Join("\n", meds.Select(s => s.ToString())));
		}
		[Test]
		public void IndexCreation()
		{
			var questionList = Program.DefaultQuestionList;

			var medicaments = new Medicaments(Program.DefaultMyStemmer, Program.MedicamentsFileName);

			var meds =
				medicaments.FindMedicamentsInTexts(questionList.GetAllAnswers().Select(a => Tuple.Create(a.QuestionId, a.Text))).ToList();
			File.WriteAllLines("MedicamentsIndex.txt", meds.Select(s => s.ToString()));
		}
	}
}
