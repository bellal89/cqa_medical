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
	public class Mkb10
	{
		const string SerializedMappingFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseases.txt";
		const string DeseaseNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseaseNames.txt";
		const string MedicamentNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-medicamentNames.txt";

		private readonly List<Question> medicalQuestions;
		
		private readonly Dictionary<int, Mkb10Desease> idToDesease;
		private readonly Dictionary<int, Mkb10TradeName> idToTradeName;
		private readonly Dictionary<int, List<int>> desToTnMapping;

		public Mkb10()
		{
			medicalQuestions =
				Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness", "treatment", "kidhealth", "doctor").
					GetAllQuestions().ToList();

			desToTnMapping = ObjectSerializer.GetFromFile<Dictionary<int, List<int>>>(SerializedMappingFileName);

			idToTradeName =
				File.ReadAllLines(MedicamentNamesFileName).Select(line => line.Split('\t')).Where(parts => parts.Length > 1).ToDictionary
					(parts => int.Parse(parts[0]), parts => new Mkb10TradeName(parts[1], parts.Length > 2 ? parts[2] : ""));

			var deseaseParts =
				File.ReadAllLines(DeseaseNamesFileName).Select(line => line.Split('\t')).Where(parts => parts.Length > 2).Select(
					parts => Tuple.Create(int.Parse(parts[0]), parts)).ToList();

			var idDeseaseList =
				deseaseParts.Where(it => it.Item2.Length > 3).Select(
					it => Tuple.Create(it.Item1,
					                   new Mkb10Desease(it.Item2[1],
					                                    desToTnMapping[it.Item1].Where(idToTradeName.ContainsKey).Select(
					                                    	id => idToTradeName[id]).ToList(),
					                                    it.Item2[3].Split('+').ToList()))).ToList();

			idDeseaseList.AddRange(
				deseaseParts.Where(it => it.Item2.Length <= 3).Select(
					it => Tuple.Create(it.Item1,
					                   new Mkb10Desease(it.Item2[1], desToTnMapping[it.Item1].Select(id => idToTradeName[id]).ToList()))));

			idToDesease = idDeseaseList.ToDictionary(it => it.Item1, it => it.Item2);
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

		private IEnumerable<Tuple<int, InvertedIndexUnit>> GetDeseasesFuzzyIndex()
		{
			var deseaseNames = idToDesease.Select(des => { var names = des.Value.Synonyms; names.Add(des.Value.Name);
			                                          	return Tuple.Create(des.Key, names);
			}).ToList();
			
			var stemmer = new MyStemmer("Mkb10Deseases.stemmed.txt", deseaseNames.SelectMany(it => it.Item2));
			var stemmedDeseaseNames =
				deseaseNames.Select(it => Tuple.Create(it.Item1, it.Item2.Select(name => String.Join(" ", name.SplitIntoWords().Select(stemmer.Stem))))).ToList();

			var desWords = stemmedDeseaseNames.SelectMany(it => it.Item2).SelectMany(name => name.SplitIntoWords());
			var questionTexts = medicalQuestions.Select(q => Tuple.Create(q.Id, q.WholeText));
			var wordsIndex = new FuzzyIndex(questionTexts, desWords);

			var deseaseIndex = new List<Tuple<int, InvertedIndexUnit>>();

			foreach (var it in stemmedDeseaseNames)
			{
				var desId = it.Item1;
				var names = it.Item2;
				var deseaseIds = new List<long>();
				foreach (var name in names)
				{
					deseaseIds.AddRange(GetUnitsIntersection(wordsIndex.GetIndex(), name.SplitIntoWords().ToArray()));
				}
				if(deseaseIds.Any())
					deseaseIndex.Add(Tuple.Create(desId, new InvertedIndexUnit(idToDesease[desId].Name, deseaseIds)));
			}
			return deseaseIndex;
		}

		private List<Tuple<int, InvertedIndexUnit>> GetMedicamentsFuzzyIndex()
		{
			var medNames = idToTradeName.Select(med => Tuple.Create(med.Key, med.Value.Name)).ToList();
			var stemmer = new MyStemmer("Mkb10TradeNames.stemmed.txt", medNames.Select(it => it.Item2));
			var stemmedTnNames =
				medNames.Select(it => Tuple.Create(it.Item1, String.Join(" ", it.Item2.SplitIntoWords().Select(stemmer.Stem)))).ToList();

			var tnWords = stemmedTnNames.Select(it => it.Item2).SelectMany(name => name.SplitIntoWords());
			var answerTexts = medicalQuestions.SelectMany(q => q.GetAnswers()).Select(a => Tuple.Create(a.QuestionId, a.Text));
			var wordsIndex = new FuzzyIndex(answerTexts, tnWords);

			return (from it in medNames
			        let medId = it.Item1
			        let name = it.Item2
			        let medIds = GetUnitsIntersection(wordsIndex.GetIndex(), name.SplitIntoWords().ToArray()).ToList()
			        where medIds.Any()
			        select Tuple.Create(medId, new InvertedIndexUnit(idToTradeName[medId].Name, medIds))).ToList();
		}

		public List<Tuple<int, int, int>> GetDesMedPairs()
		{
			var desIndex = GetDeseasesFuzzyIndex();
			var medIndex = GetMedicamentsFuzzyIndex();
			return (from med in medIndex
			        from des in desIndex
			        let intersection = des.Item2.Ids.Intersect(med.Item2.Ids).ToList()
			        where intersection.Any()
			        select Tuple.Create(des.Item1, med.Item1, intersection.Count)).ToList();
		}

		public IEnumerable<Tuple<int, int>> GetCorrectDesMedPairs()
		{
			return desToTnMapping.SelectMany(kv => kv.Value.Select(med => Tuple.Create(kv.Key, med)));
		}
	}

	[TestFixture]
	public class Mkb10Test
	{
		private readonly Mkb10 mkb = new Mkb10();

		[Test]
		public void TestLoading()
		{
			var correctPairs = new HashSet<Tuple<int, int>>(mkb.GetCorrectDesMedPairs());
			Console.WriteLine("Correct pairs count: " + correctPairs.Count);

			var pairs = mkb.GetDesMedPairs();
			Console.WriteLine("Extracted pairs count: " + pairs.Count);

			var correctExtracted = pairs.Where(pair => correctPairs.Contains(Tuple.Create(pair.Item1, pair.Item2))).ToList();
			Console.WriteLine("Correct extracted count: " + correctExtracted.Count);
			Console.WriteLine("Fraction of correct in extracted: " +  (double)correctExtracted.Count / pairs.Count);
			Console.WriteLine("Sum fraction: " + (double)correctExtracted.Sum(it => it.Item3) / pairs.Sum(it => it.Item3));
		}
	}
}
