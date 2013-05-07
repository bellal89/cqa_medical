using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.SpellChecker;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Mkb10
{
	public class Mkb10
	{
		const string SerializedMappingFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseases.txt";
		const string DeseaseNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseaseNames.txt";
		const string MedicamentNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-medicamentNames.txt";

		private static readonly HashSet<string> BadDeseaseWords =
			new HashSet<string>(
				"cимптом признаки диагностика дополнительные источник носительство необходимость опасность обращения последствия факторы причины практика"
					.SplitIntoWords());

		private readonly List<Question> medicalQuestions;
		
		private readonly Dictionary<int, Mkb10Desease> idToDesease;
		private readonly Dictionary<int, Mkb10TradeName> idToTradeName;
		private readonly Dictionary<int, List<int>> desToTnMapping;
		private readonly Dictionary<int, List<string>> desToSubstanceMapping; 
		
		public Mkb10()
		{
			medicalQuestions =
				Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness", "treatment", "kidhealth", "doctor").
					GetAllQuestions().ToList();

			desToTnMapping = ObjectSerializer.GetFromFile<Dictionary<int, List<int>>>(SerializedMappingFileName);

			idToTradeName =
				File.ReadAllLines(MedicamentNamesFileName).Select(line => line.Split('\t')).Where(parts => parts.Length > 1).ToDictionary
					(parts => int.Parse(parts[0]), parts => new Mkb10TradeName(parts[1], GetActiveSubstance(parts)));

			desToSubstanceMapping = desToTnMapping.ToDictionary(kv => kv.Key,
			                                                    kv =>
			                                                    kv.Value.Where(idToTradeName.ContainsKey).Select(medId => idToTradeName[medId].ActiveSubstance).
			                                                    	Distinct().ToList());

			var deseaseParts =
				File.ReadAllLines(DeseaseNamesFileName).Select(line => line.Split('\t')).Where(parts => parts.Length > 2).Select(
					parts => Tuple.Create(int.Parse(parts[0]), parts)).ToList();

			var idDeseaseList =
				deseaseParts.Where(it => it.Item2.Length > 3).Select(
					it => Tuple.Create(it.Item1,
					                   new Mkb10Desease(CleanName(it.Item2[1]),
					                                    desToTnMapping[it.Item1].Where(idToTradeName.ContainsKey).Select(
					                                    	id => idToTradeName[id]).ToList(),
					                                    it.Item2[3].Split('+').ToList()))).ToList();

			idDeseaseList.AddRange(
				deseaseParts.Where(it => it.Item2.Length <= 3).Select(
					it => Tuple.Create(it.Item1,
					                   new Mkb10Desease(CleanName(it.Item2[1]), desToTnMapping[it.Item1].Select(id => idToTradeName[id]).ToList()))));

			idToDesease =
				idDeseaseList.Where(it => !it.Item2.Name.SplitIntoWords().Any(BadDeseaseWords.Contains)).ToDictionary(
					it => it.Item1, it => it.Item2);
			desToTnMapping = desToTnMapping.Where(kv => idToDesease.ContainsKey(kv.Key)).ToDictionary(kv => kv.Key,
			                                                                                          kv => kv.Value);
		}

		private static string GetActiveSubstance(IList<string> parts)
		{
			if (parts.Count <= 2)
				return "";
			var substance = parts[2].ToLower().Split('+');
			if (substance.Length == 2 && Regex.IsMatch(substance[1], @"[a-z\s]+"))
				return substance[0];
			return parts[2];
		}

		public Mkb10Desease GetDesease(int id)
		{
			return idToDesease[id];
		}

		public Mkb10TradeName GetTradeName(int id)
		{
			return idToTradeName[id];
		}

		private static string CleanName(string name)
		{
			var parts = name.SplitIntoWords().ToList();
			return parts.Count <= 1 ? name : String.Join(" ", parts.Where(part => !part.Any(Char.IsDigit)));
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

		public IEnumerable<Tuple<int, InvertedIndexUnit>> GetDeseasesFuzzyIndex()
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

		public IEnumerable<Tuple<int, InvertedIndexUnit>> GetMedicamentsFuzzyIndex()
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

		public List<Tuple<int, string, int>> GetDesMedPairs()
		{
			var desIndex = GetDeseasesFuzzyIndex();
			var activeSubIndex = GetDesActiveSubIndex();

			return (from med in activeSubIndex
			        from des in desIndex
			        let intersection = des.Item2.Ids.Intersect(med.Item2.Ids).ToList()
			        where intersection.Any()
			        select Tuple.Create(des.Item1, med.Item1, intersection.Count)).ToList();
		}

		public IEnumerable<Tuple<string, InvertedIndexUnit>> GetDesActiveSubIndex()
		{
			var medIndex = GetMedicamentsFuzzyIndex();
			return medIndex.GroupBy(it => idToTradeName[it.Item1].ActiveSubstance,
							 (key, items) =>
							 Tuple.Create(key, new InvertedIndexUnit(key, items.SelectMany(it => it.Item2.Ids).Distinct())));
		}

		public IEnumerable<Tuple<int, int>> GetCorrectDesMedPairs()
		{
			return desToTnMapping.SelectMany(kv => kv.Value.Select(med => Tuple.Create(kv.Key, med)));
		}

		public IEnumerable<Tuple<int, string>> GetCorrectDesSubstancePairs()
		{
			return desToSubstanceMapping.SelectMany(kv => kv.Value.Select(sub => Tuple.Create(kv.Key, sub)));
		}
	}

	[TestFixture]
	public class Mkb10Test
	{
		//private readonly Mkb10 mkb = new Mkb10();

		[Test]
		public void TestLoading()
		{
			var mkb = new Mkb10();

			var correctPairs = new HashSet<Tuple<int, string>>(mkb.GetCorrectDesSubstancePairs());
			var extracted = mkb.GetDesMedPairs();
			var correctExtracted = extracted.Where(pair => correctPairs.Contains(Tuple.Create(pair.Item1, pair.Item2))).ToList();

			var res = new List<string>
			          	{
			          		"Correct pairs count: " + correctPairs.Count,
			          		"Extracted pairs count: " + extracted.Count,
			          		"Correct extracted count: " + correctExtracted.Count,
			          		"",
			          		"Fraction of correct in extracted: " + (double) correctExtracted.Count/extracted.Count,
			          		"Sum fraction: " + (double) correctExtracted.Sum(it => it.Item3)/extracted.Sum(it => it.Item3),
			          		"Fraction of >1: " +
			          		(double) correctExtracted.Count(it => it.Item3 > 1)/extracted.Count(it => it.Item3 > 1),
			          		"Fraction of >2: " +
			          		(double) correctExtracted.Count(it => it.Item3 > 2)/extracted.Count(it => it.Item3 > 2),
			          		"Fraction of >10: " +
			          		(double) correctExtracted.Count(it => it.Item3 > 10)/extracted.Count(it => it.Item3 > 10),
			          		">10 count: " + extracted.Count(it => it.Item3 > 10),
			          		"Fraction of >50: " +
			          		(double) correctExtracted.Count(it => it.Item3 > 50)/extracted.Count(it => it.Item3 > 50),
			          		">50 count: " + extracted.Count(it => it.Item3 > 50)
			          	};

			Console.WriteLine(String.Join("\n", res));
			File.WriteAllLines("PairsDesToActiveSubstance.txt", res);

			File.WriteAllLines("PairsDesToActiveSubstanceFractionsBy25.txt",
			                   extracted.GroupBy(pair => pair.Item3/25,
			                                     (key, vals) =>
			                                     Tuple.Create(key, vals.Count(),
			                                                  vals.Count(
			                                                  	val => correctPairs.Contains(Tuple.Create(val.Item1, val.Item2)))))
			                   	.OrderBy(it => it.Item1)
			                   	.Select(
									it => (it.Item1 * 25 + 1) + " to " + (it.Item1 * 25 + 25) + "\t" + it.Item3 + "\t" + (it.Item2 - it.Item3)));
		}
	}
}
