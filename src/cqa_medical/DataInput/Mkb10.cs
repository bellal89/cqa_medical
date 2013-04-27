using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.SpellChecker;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.DataInput
{
	public static class Mkb10
	{
		const string DeseasesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseases.txt";
		const string DeseaseNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseaseNames.txt";
		const string MedicamentNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-medicamentNames.txt";

		private static readonly List<Question> MedicalQuestions;

		static Mkb10()
		{
			 MedicalQuestions =
			Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness", "treatment", "kidhealth", "doctor").
				GetAllQuestions().ToList();
		}

		private static FuzzyIndex GetWordFuzzyIndex(IEnumerable<string> names)
		{
			var questionTexts = MedicalQuestions.Select(q => Tuple.Create(q.Id, q.WholeText));

			return new FuzzyIndex(questionTexts, names.SelectMany(name => name.SplitIntoWords()));
		}

		public static List<InvertedIndexUnit> GetDeseasesFuzzyIndex()
		{
			var deseases = LoadDeseases(DeseasesFileName, DeseaseNamesFileName, MedicamentNamesFileName);
			var deseaseNames = deseases.Select(des => { var names = des.Synonyms; names.Add(des.Name);
			                                          	return Tuple.Create(des.Name, names);
			}).ToList();
			

			var stemmer = new MyStemmer("Mkb10Deseases.stemmed.txt", deseaseNames.SelectMany(it => it.Item2));
			var stemmedDeseaseNames =
				deseaseNames.Select(it => Tuple.Create(it.Item1, it.Item2.Select(name => String.Join(" ", name.SplitIntoWords().Select(stemmer.Stem))))).ToList();

			var wordsIndex = GetWordFuzzyIndex(stemmedDeseaseNames.SelectMany(it => it.Item2));

			

			var deseaseIndex = new List<InvertedIndexUnit>();

			foreach (var it in stemmedDeseaseNames)
			{
				var desName = it.Item1;
				var names = it.Item2;
				var deseaseIds = new List<long>();
				foreach (var name in names)
				{
					deseaseIds.AddRange(GetUnitsIntersection(wordsIndex.GetIndex(), name.SplitIntoWords().ToArray()));
				}
				if(deseaseIds.Any())
					deseaseIndex.Add(new InvertedIndexUnit(desName, deseaseIds));
			}
			return deseaseIndex;
		}

		private static IEnumerable<long> GetUnitsIntersection(IEnumerable<InvertedIndexUnit> index, params string[] items)
		{
			var itemsSet = new HashSet<string>(items);
			var units = index.Where(unit => itemsSet.Contains(unit.Word)).ToList();
			
			if (!itemsSet.Any() || itemsSet.Except(units.Select(unit => unit.Word)).Any())
				return new HashSet<long>();

			var seed = units.First().Ids;
			return units.Skip(1).Aggregate(seed, (current, unit) => new HashSet<long>(current.Intersect(unit.Ids)));
		}

		private static IEnumerable<Mkb10Desease> LoadDeseases(string serializedMappingFileName, string deseaseNamesFileName, string tradeNamesFileName)
		{
			var mapping = ObjectSerializer.GetFromFile<Dictionary<int, List<int>>>(serializedMappingFileName);
			var tradeNames =
				File.ReadAllLines(tradeNamesFileName).Select(line => line.Split('\t')).Where(parts => parts.Length > 1).ToDictionary
					(parts => int.Parse(parts[0]), parts => new Mkb10TradeName(parts[1], parts.Length > 2 ? parts[2] : ""));

			var deseaseParts = File.ReadAllLines(deseaseNamesFileName).Select(line => line.Split('\t')).Where(parts => parts.Length > 2).Select(parts => Tuple.Create(int.Parse(parts[0]), parts)).ToList();
			var deseasesList =
				deseaseParts.Where(it => it.Item2.Length > 3).Select(
					it =>
					new Mkb10Desease(it.Item2[1], mapping[it.Item1].Where(tradeNames.ContainsKey).Select(id => tradeNames[id]).ToList(),
					                              it.Item2[3].Split('+').ToList())).ToList();

			deseasesList.AddRange(
				deseaseParts.Where(it => it.Item2.Length <= 3).Select(
					it =>
					new Mkb10Desease(it.Item2[1], mapping[it.Item1].Select(id => tradeNames[id]).ToList())));
			return deseasesList;
		}
	}

	[TestFixture]
	public class Mkb10Test
	{
		[Test]
		public static void TeslLoading()
		{
			var index = Mkb10.GetDeseasesFuzzyIndex();
			Console.WriteLine(index.Count);
		}
	}
}
