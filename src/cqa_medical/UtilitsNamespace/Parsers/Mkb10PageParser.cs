﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.SpellChecker;
using cqa_medical.Statistics;

namespace cqa_medical.UtilitsNamespace.Parsers
{
	class Mkb10PageParser : HTMLPageParser<Mkb10Desease>
	{
		private readonly HtmlDocument html = new HtmlDocument();
		private readonly HashSet<string> uniqueDeseases = new HashSet<string>();

		public Mkb10PageParser(string pagesDirectory) : base(pagesDirectory)
		{
		}

		protected override bool IsUnique(Mkb10Desease page)
		{
			if (uniqueDeseases.Contains(page.Name))
				return false;
			uniqueDeseases.Add(page.Name);
			return true;
		}

		protected override Mkb10Desease ParsePage(string fileName)
		{
			html.Load(fileName, Encoding.GetEncoding(1251));
			var breadcrumbs = html.GetElementbyId("breadcrumbs");
			if (breadcrumbs == null)
			{
				Console.WriteLine("Breadcrumbs not found: " + fileName);
				return null;
			}
			var name = breadcrumbs.LastChild.InnerText.Trim().ToLower();
			if (name.StartsWith("&gt;")) name = name.Remove(0, 4).TrimStart();
			var content = html.GetElementbyId("content");
			var tradeNodes = content.SelectNodes("table[@class='rest_nest']/tr/td[@class='rest_data']");
			if (tradeNodes == null)
			{
				Console.WriteLine("Trade names not found.");
				Console.WriteLine("File: " + fileName);
				Console.WriteLine("Name: " + name);
				return null;
			}
			var tradeNames = new List<Mkb10TradeName>();
			for (var i = 0; i < tradeNodes.Count - 1; i += 2)
			{
				var tradeName = tradeNodes[i].ChildNodes.FindFirst("a");
				if (tradeName == null) continue;
				var tradeSubstance = tradeNodes[i+1].ChildNodes.FindFirst("a");
				tradeNames.Add(new Mkb10TradeName(tradeName.InnerText.ToLower().Trim().TrimEnd('®'), tradeSubstance != null ? tradeSubstance.InnerText.ToLower().Trim().TrimEnd('®') : ""));
			}

			var synonymsNode = html.GetElementbyId("synonyms");
			return synonymsNode == null
			       	? new Mkb10Desease(name, tradeNames)
			       	: new Mkb10Desease(name, tradeNames, synonymsNode.SelectNodes("ul/li").Select(li => li.InnerText.Trim().ToLower()).ToList());
		}

		public static FuzzyIndex GetDeseaseStemmedFuzzyIndex(List<Mkb10Desease> deseases)
		{
			var mostFrequentWords = new HashSet<string>(deseases.SelectMany(des => des.Name.SplitIntoWords())
																.GroupBy(desWord => desWord, (key, words) => Tuple.Create(key, words.Count()))
																.Where(it => it.Item2 >= 30)
																.Select(it => it.Item1));
			var defaultQuestionList = Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness", "treatment", "kidhealth", "doctor");
			var stemmedFuzzyIndex =
				new FuzzyIndex(defaultQuestionList.GetAllQuestions().Select(q => Tuple.Create(q.Id, q.WholeText)),
				               deseases.SelectMany(
				               	des =>
				               	des.GetAllNames().Select(name => name.SplitIntoWords().Where(w => !mostFrequentWords.Contains(w))).
				               		Where(words => words.Any()).SelectMany(words => words)));
			
			return stemmedFuzzyIndex;
		}

		public static FuzzyIndex GetTradeNameStemmedFuzzyIndex(List<Mkb10Desease> deseases)
		{
			var mostFrequentWords = new HashSet<string>(deseases.SelectMany(des => des.TradeNames.SelectMany(tn => tn.Name.SplitIntoWords()) )
													.GroupBy(word => word, (key, words) => Tuple.Create(key, words.Count()))
													.Where(it => it.Item2 >= 70)
													.Select(it => it.Item1));

			var allTradeNameWords = deseases.SelectMany(des => des.TradeNames.SelectMany(name => name.Name.SplitIntoWords())).ToList();
			allTradeNameWords.AddRange(
				deseases.SelectMany(des => des.TradeNames.SelectMany(name => name.ActiveSubstances.SelectMany(substance => substance.SplitIntoWords()))));

			var defaultQuestionList = Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness", "treatment", "kidhealth", "doctor");
			var stemmedFuzzyIndex =
				new FuzzyIndex(defaultQuestionList.GetAllAnswers().Select(a => Tuple.Create(a.QuestionId, a.Text)), allTradeNameWords.Where(w => !mostFrequentWords.Contains(w)));
			return stemmedFuzzyIndex;
		}

		public static IEnumerable<Mkb10Desease> GetStemmedDeseases(IEnumerable<Mkb10Desease> mkb10Deseases, string pagesDirectory)
		{
			var fileNamesToStemmer = Directory.GetFiles(pagesDirectory).Where(fileName => !fileName.EndsWith(MyStemLoader.GetStemmedFileSuffix())).ToArray();
			var stemmer = new MyStemmer(fileNamesToStemmer);

			return (from des in mkb10Deseases
					let stemmedName = StemString(stemmer, des.Name)
					let stemmedSynonims = des.Synonyms.Select(syn => StemString(stemmer, syn)).ToList()
					let stemmedTradeNames =
						des.TradeNames.Select(
							tn => new Mkb10TradeName(StemString(stemmer, tn.Name), String.Join("+", tn.ActiveSubstances.Select(substance => StemString(stemmer, substance))))).ToList()
					select new Mkb10Desease(stemmedName, stemmedTradeNames, stemmedSynonims)).ToList();
		}

		public IEnumerable<Mkb10Desease> GetStemmedDeseases()
		{
			return GetStemmedDeseases(GetObjects(), GetPagesDirectory());
		}

		private static string StemString(IStemmer stemmer, string s)
		{
			return String.Join(" ", s.SplitIntoWords().Select(stemmer.Stem));
		}

		public static IEnumerable<InvertedIndexUnit> GetMkb10DeseasesFuzzyIndex(List<Mkb10Desease> deseases)
		{
			var index = GetDeseaseStemmedFuzzyIndex(deseases);

			var deseasesToQuestions = new Dictionary<int, List<long>>();
			for (var i = 0; i < deseases.Count; i++)
			{
				var desease = deseases[i];
				foreach (var name in desease.GetAllNames())
				{
					var desQuestions = index.GetIntersectionByWords(name.SplitIntoWords().ToList()).ToList();

					if (!deseasesToQuestions.ContainsKey(i))
						deseasesToQuestions[i] = new List<long>();
					deseasesToQuestions[i].AddRange(desQuestions);
				}
			}
			return deseases.Select((des, i) => new InvertedIndexUnit(des.Name + " (" + String.Join(", ", des.Synonyms) + ")", deseasesToQuestions[i]));
		}

		public IEnumerable<InvertedIndexUnit> GetMkb10DeseasesFuzzyIndex()
		{
			return GetMkb10DeseasesFuzzyIndex(GetStemmedDeseases().ToList());
		}

		public static IEnumerable<InvertedIndexUnit> GetMkb10MedicamentsFuzzyIndex(List<Mkb10Desease> deseases)
		{
			var index = GetTradeNameStemmedFuzzyIndex(deseases);
			var substanceToQuestions = new Dictionary<string, List<long>>();

			foreach (var tradeName in deseases.SelectMany(des => des.TradeNames))
			{
				var substance = tradeName.Name + "(" + String.Join(", ", tradeName.ActiveSubstances) + ")";
				//				var substance = tradeName.GetShortName();
				//				if (tradeName.ActiveSubstances.Any())
				//					substance = String.Join(" + ", tradeName.ActiveSubstances);

				foreach (var name in tradeName.GetAllNames())
				{
					var tnQuestions = index.GetIntersectionByWords(name.SplitIntoWords().ToList());
					if (!substanceToQuestions.ContainsKey(substance))
						substanceToQuestions[substance] = new List<long>();
					substanceToQuestions[substance].AddRange(tnQuestions);
				}
			}

			return substanceToQuestions.Select(sq => new InvertedIndexUnit(sq.Key, sq.Value.Distinct()));
		}

		public IEnumerable<InvertedIndexUnit> GetMkb10MedicamentsFuzzyIndex()
		{
			return GetMkb10MedicamentsFuzzyIndex(GetStemmedDeseases().ToList());
		}

		public void ExportData(string deseasesFile, string deseaseNamesFile, string medicamentNamesFile)
		{
			var deseases = GetObjects();

			var desId = 0;
			var deses = new Dictionary<string, int>();
			var medId = 0;
			var meds = new Dictionary<string, int>();
			var desToMeds = new Dictionary<int, List<int>>();

			foreach (var des in deseases)
			{
				var desName = des.Name + "\t" + String.Join("+", des.Synonyms);
				if (!deses.ContainsKey(desName))
					deses[desName] = desId++;
				
				desToMeds[deses[desName]] = new List<int>();
				foreach (var tnName in des.TradeNames.Select(tn => tn.Name + "\t" + String.Join("+", tn.ActiveSubstances)))
				{
					if (!meds.ContainsKey(tnName))
						meds[tnName] = medId++;
					desToMeds[deses[desName]].Add(meds[tnName]);
				}
			}
			ObjectSerializer.SaveToFile(desToMeds, deseasesFile);
			File.WriteAllLines(deseaseNamesFile, deses.OrderBy(kv => kv.Key).Select(kv => kv.Value + "\t" + kv.Key));
			File.WriteAllLines(medicamentNamesFile, meds.OrderBy(kv => kv.Key).Select(kv => kv.Value + "\t" + kv.Key));
		}

		public static IEnumerable<Mkb10Desease> GetDeseasesFrom (string deseasesFile, string deseaseNamesFile, string medicamentsFile)
		{
			var desToMeds = ObjectSerializer.GetFromFile<Dictionary<int, List<int>>>(deseasesFile);
			var deses =
				File.ReadAllLines(deseaseNamesFile).Select(s => s.Split('\t')).Where(parts => parts.Length == 3).ToDictionary(
					parts => int.Parse(parts[0]), parts => Tuple.Create(parts[1], parts[2].Split(new[]{'+'}, StringSplitOptions.RemoveEmptyEntries)));
			var meds =
				File.ReadAllLines(medicamentsFile).Select(s => s.Split('\t')).Where(parts => parts.Length == 3).ToDictionary(
					parts => int.Parse(parts[0]), parts => Tuple.Create(parts[1], parts[2]));
			return desToMeds.Select(
				des =>
				new Mkb10Desease(deses[des.Key].Item1,
				                 des.Value.Select(medId => new Mkb10TradeName(meds[medId].Item1, meds[medId].Item2)).ToList(),
				                 deses[des.Key].Item2.ToList())).ToList();
		}

		private HashSet<string> GetTradeNameStopWords()
		{
			var statistics = new Statistics.Statistics(Program.DefaultQuestionList);
			var frequencies = statistics.WordFrequencyDistribution(Program.DefaultMyStemmer);

			return new HashSet<string>(GetStemmedDeseases().SelectMany(
					des => des.TradeNames.Where(tn => !tn.ActiveSubstances.Any()).Select(tn => tn.GetShortName())).Distinct().Select(
						med =>
						Tuple.Create(med,
						             frequencies.ContainsKey(med) ? frequencies[med] : 0)).
				                    	Where(it => it.Item2 > 1000).Select(it => it.Item1));
		}
	}

	[Serializable]
	internal class Mkb10Desease
	{
		public string Name { get; set; }
		public readonly List<Mkb10TradeName> TradeNames;
		public readonly List<string> Synonyms; 

		public Mkb10Desease(string name, List<Mkb10TradeName> tradeNames)
		{
			Name = name;
			TradeNames = tradeNames;
			Synonyms = new List<string>();
		}

		public Mkb10Desease(string name, List<Mkb10TradeName> tradeNames, List<string> synonyms)
		{
			Name = name;
			TradeNames = tradeNames;
			Synonyms = synonyms;
		}

		public IEnumerable<string> GetAllNames()
		{
			var names = new List<string> {Name};
			names.AddRange(Synonyms);
			return names;
		}
	}

	[Serializable]
	internal class Mkb10TradeName
	{
		public readonly string Name;
		public readonly List<string> ActiveSubstances = new List<string>();

		public Mkb10TradeName(string name, string activeSubstance)
		{
			Name = name;
			ActiveSubstances.AddRange(activeSubstance.Split('+').Select(s => s.EndsWith("()") ? s.Trim(new[] { '(', ')', '\n', ' ' }) : s).Select(item => item.Trim().TrimEnd('®')));
		}

		public IEnumerable<string> GetAllNames()
		{
			var names = new List<string> { GetShortName() };
			if(ActiveSubstances.Any())
				names.AddRange(ActiveSubstances);
			return names;
		}

		public string GetShortName()
		{
			var name = Name;
			var names = Name.SplitIntoWords().ToList();
			if (names.Count() > 1)
			{
				name = names.First();
			}
			return name;
		}
	}

	[TestFixture]
	public class Mkb10Test
	{
		private Mkb10PageParser parser;
		
		[SetUp]
		public void Initialize()
		{
			parser = new Mkb10PageParser(Program.FilesDirectory + "Mkb10/");
		}

		[Test]
		public void TestCreation()
		{
			var deseases = parser.GetStemmedDeseases();

			var statistics = new Statistics.Statistics(Program.DefaultQuestionList);
			var freqs = statistics.WordFrequencyDistribution(Program.DefaultMyStemmer);

			var tnWords = deseases.SelectMany(des => des.TradeNames.SelectMany(tn => tn.ActiveSubstances.SelectMany(substance => substance.SplitIntoWords())).GroupBy(tn => tn,
			                                                                                                         (key, tns)
			                                                                                                         =>
			                                                                                                         Tuple.Create
			                                                                                                         	(key,
			                                                                                                         	 tns.Count(), freqs.ContainsKey(key) ? freqs[key] : 0)));

			Console.WriteLine(String.Join("\n", tnWords.OrderByDescending(tn => tn.Item2).Take(100).Select(tn => tn.Item1 + "\t" + tn.Item2 + "\t" + tn.Item3)));
			Console.WriteLine("=======");
			Console.WriteLine(String.Join("\n", tnWords.OrderByDescending(tn => tn.Item3).Take(100).Select(tn => tn.Item1 + "\t" + tn.Item2 + "\t" + tn.Item3)));
		}

		[Test]
		public void TestMkb10DeseaseWords()
		{
			var deseases = parser.GetStemmedDeseases();

			Console.WriteLine(String.Join("\n",
			            deseases.SelectMany(des => des.TradeNames.SelectMany(tn => tn.Name.SplitIntoWords()))
			                                	.GroupBy(word => word, (key, words) => Tuple.Create(key, words.Count())).
			                                	OrderByDescending(it => it.Item2).Select(it => it.Item1 + "\t" + it.Item2)));
		}

		[Test]
		public void TestDeseasesExporting()
		{
			const string desFileName = "TestDeseasesSerialized.txt";
			const string desNamesFileName = "TestDeseaseNamesSerialized.txt";
			const string medNamesFileName = "TestMedicamentNamesSerialized.txt";
			parser.ExportData(desFileName, desNamesFileName, medNamesFileName);

			var oldDeseases = parser.GetObjects().GroupBy(des => des.Name, (key, deses) => deses.Last()).ToDictionary(des => des.Name, des => des);
			var newDeseases = Mkb10PageParser.GetDeseasesFrom(desFileName, desNamesFileName, medNamesFileName).GroupBy(des => des.Name, (key, deses) => deses.Last()).Select(des => des);

			foreach (var newDes in newDeseases)
			{
				var oldDes = oldDeseases[newDes.Name];
				Assert.AreEqual(newDes.Name, oldDes.Name);
				foreach (var syn in newDes.Synonyms)
				{
					Assert.Contains(syn, oldDes.Synonyms);
				}
				foreach (var tn in newDes.TradeNames)
				{
					Assert.Contains(tn.Name, oldDes.TradeNames.Select(n => n.Name).ToList());
					foreach (var substance in tn.ActiveSubstances)
					{
						Assert.Contains(substance, oldDes.TradeNames.SelectMany(n => n.ActiveSubstances).ToList());
					}
				}
			}
		}
	}

}