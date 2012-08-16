using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.BodyAnalisys
{
	internal class Medicaments
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
				.Select(line => line.Split(new[] {'\t'}))
				.Where(item => item.Length == 3))
			{
				AddToIndex(index, parts[0], i);
				//AddToIndex(index, parts[1], i);
				i++;
			}
			return Filter(index);
		}

		private static Dictionary<string, HashSet<int>> Filter(Dictionary<string, HashSet<int>> index)
		{
			return index.Where(item => item.Value.Count <= 85 && item.Key.Length > 2).ToDictionary(item => item.Key,
			                                                                                       item => item.Value);
		}

		private void AddToIndex(IDictionary<string, HashSet<int>> index, string medicamentFullName, int medicamentId)
		{
			var name = medicamentFullName.SplitIntoWords().FirstOrDefault();

			if (string.IsNullOrEmpty(name)) return;

			var stemmedName = stemmer.Stem(name);
			if (!index.ContainsKey(stemmedName))
			{
				index.Add(stemmedName, new HashSet<int>());
			}
			index[stemmedName].Add(medicamentId);
		}

		public override string ToString()
		{
			return String.Join("\n",
			                   invertedIndex.OrderByDescending(item => item.Value.Count).Select(
			                   	item =>
			                   	item.Value.Count + "\t" + item.Key + "\t" + String.Join(" ", item.Value.OrderBy(id => id))));
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
				var words = idAndText.Item2.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
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

			return DataActualityChecker.Check(
				new Lazy<InvertedIndexUnit[]>(() =>
				                              	{
				                              		var questionList = Program.DefaultQuestionList;
				                              		var medicaments = new Medicaments(Program.DefaultMyStemmer,
				                              		                                  Program.MedicamentsFileName);
				                              		return medicaments
				                              			.FindMedicamentsInTexts(questionList
				                              			                        	.GetAllAnswers()
				                              			                        	.Select(a => Tuple.Create(a.QuestionId, a.Text)))
				                              			.ToArray();
				                              	}),
				InvertedIndexUnit.FormatStringWrite,
				InvertedIndexUnit.FormatStringParse,
				new FileDependencies(
					Program.MedicamentsIndexFileName,
					Program.MedicamentsFileName));
		}
	}

	[TestFixture]
	public class MedicamentsTest
	{
		[Test]
		public void CreationTest()
		{
			var medicaments = new Medicaments(Program.DefaultMyStemmer,Program.MedicamentsFileName);

			Console.WriteLine(medicaments);
			File.WriteAllText("MedOutput2.txt", medicaments.ToString());
		}

		[Test, Explicit]
		public void IndexTest()
		{
			var questionList = Program.TestDefaultQuestionList;

			var medicaments = new Medicaments(Program.DefaultMyStemmer, Program.MedicamentsFileName);

			var meds =
				medicaments.FindMedicamentsInTexts(questionList.GetAllAnswers().Select(a => Tuple.Create(a.QuestionId, a.Text))).ToList();
			Console.WriteLine(String.Join("\n", meds.Select(s => s.ToString())));
		}
		[Test, Explicit]
		public void IndexCreation()
		{
			var questionList = Program.DefaultQuestionList;

			var medicaments = new Medicaments(Program.DefaultMyStemmer, Program.MedicamentsFileName);

			var meds =
				medicaments.FindMedicamentsInTexts(questionList.GetAllAnswers().Select(a => Tuple.Create(a.QuestionId, a.Text))).ToList();
			//File.WriteAllLines("MedicamentsIndex.txt", meds.OrderByDescending(q => q.Ids.Count).Select(s => s.ToString()));
			File.WriteAllLines("MedicamentsIndex.txt", meds.OrderByDescending(q => q.Ids.Count).Select(s => s.Word + "\t" + s.Ids.Count));
		}
	}
}
