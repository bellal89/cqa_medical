using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.BodyAnalisys;
using cqa_medical.UtilitsNamespace;
using cqa_medical.UtilitsNamespace.Parsers;

namespace cqa_medical.Statistics
{
	public class DeseasesToMedicamentsTable
	{
		public readonly Dictionary<Tuple<string,string>,  int> MedicalGuide;
		private readonly InvertedIndexUnit[] deseases;
		private readonly InvertedIndexUnit[] medicaments;
		private readonly int minAmount;

		private DeseasesToMedicamentsTable(Dictionary<Tuple<string, string>, int> medicalGuide)
		{
			MedicalGuide = medicalGuide;
			deseases = new InvertedIndexUnit[0];
			medicaments = new InvertedIndexUnit[0];
			minAmount = 0;
		}

		public DeseasesToMedicamentsTable( InvertedIndexUnit[] deseases, InvertedIndexUnit[] medicaments, int minAmount = 50)
		{
			MedicalGuide = new Dictionary<Tuple<string, string>, int>();
			this.deseases = deseases;
			this.medicaments = medicaments;
			this.minAmount = minAmount;
			Initialize();
		}

		public void Initialize()
		{
			foreach (var desease in deseases)
				foreach (var medicament in medicaments)
					AddToMedicalGuide(desease.Word, medicament.Word, desease.Ids.Intersect(medicament.Ids).Count());
		}

		public void AddToMedicalGuide(string desease, string medicament, int howMany = 1)
		{
			if (howMany < minAmount ) return;
			MedicalGuide.Add(Tuple.Create(desease, medicament), howMany);
		}

		public string ToStringOneToMany(int howMany)
		{
			return String.Join("\n", MedicalGuide.GroupBy(it => it.Key.Item1,
			                                       (key, items) =>
			                                       Tuple.Create(key, items.Select(it => Tuple.Create(it.Key.Item2, it.Value)))).
			                  	OrderByDescending(it => it.Item2.Sum(med => med.Item2)).Select(
			                  		it => it.Item1 + "\t\n\t" + String.Join("\n\t", it.Item2.OrderByDescending(med => med.Item2).Take(howMany).Select(med => med.Item1 + "\t" + med.Item2))));
		}

		public override string ToString()
		{
			return String.Join("\n", MedicalGuide.OrderByDescending(a => a.Value).Select(t => String.Join("\t", t.Key.Item1 , t.Key.Item2 , t.Value)));
		}

		public string CapitulatoryStringBySecond()
		{
			var q = new Dictionary<string, HashSet<Tuple<string, int>>>();
			foreach (var s in MedicalGuide.Keys.Select(key => key.Item2).Distinct())
			{
				q.Add(s,new HashSet<Tuple<string, int>>());
				foreach (var w in MedicalGuide.Keys.Where(key => key.Item2 == s))
				{
					q[s].Add(Tuple.Create(w.Item1, MedicalGuide[w]));
				} 
			}
			return String.Join("\n",
			                   q.OrderByDescending(k => k.Value.Sum(w => w.Item2))
			                   	.Select(
			                   		k =>
			                   		k.Key + "\t" +
			                   		String.Join(", ", k.Value.OrderByDescending(t => t.Item2).Select(t => String.Format("{0}({1})", t.Item1, t.Item2)))));
		}
		
		public string CapitulatoryStringByFirst()
		{
			var q = new Dictionary<string, HashSet<Tuple<string, int>>>();
			foreach (var s in MedicalGuide.Keys.Select(key => key.Item1).Distinct())
			{
				q.Add(s,new HashSet<Tuple<string, int>>());
				foreach (var w in MedicalGuide.Keys.Where(key => key.Item1 == s))
				{
					q[s].Add(Tuple.Create(w.Item2, MedicalGuide[w]));
				} 
			}
			return String.Join("\n",
			                   q.OrderByDescending(k => k.Value.Sum(w => w.Item2))
			                   	.Select(
			                   		k =>
			                   		k.Key + "\t" +
			                   		String.Join(", ", k.Value.OrderByDescending(t => t.Item2).Select(t => String.Format("{0}({1})", t.Item1, t.Item2)))));
		}

		public void ExportTo(string tableFileName, string deseasesFileName, string medicamentsFileName)
		{
			var meds = new Dictionary<string, int>();
			var deses = new Dictionary<string, int>();
			var items = new List<string>();
			var desId = 0;
			var medId = 0;
			foreach (var item in MedicalGuide)
			{
				if(!deses.ContainsKey(item.Key.Item1))
					deses[item.Key.Item1] = desId++;
				if (!meds.ContainsKey(item.Key.Item2))
					meds[item.Key.Item2] = medId++;
				items.Add(deses[item.Key.Item1] + "\t" + meds[item.Key.Item2] + "\t" + item.Value);
			}

			File.WriteAllLines(deseasesFileName, deses.Select(kv => kv.Value + "\t" + kv.Key));
			File.WriteAllLines(medicamentsFileName, meds.Select(kv => kv.Value + "\t" + kv.Key));
			File.WriteAllLines(tableFileName, items);
		}

		public static DeseasesToMedicamentsTable ImportFrom(string tableFileName, string deseasesFileName, string medicamentsFileName)
		{
			var deses =
				File.ReadAllLines(deseasesFileName).Select(s => s.Trim().Split('\t')).Where(parts => parts.Length == 2).ToDictionary
					(parts => int.Parse(parts[0]), parts => parts[1]);
			var meds =
				File.ReadAllLines(medicamentsFileName).Select(s => s.Trim().Split('\t')).Where(parts => parts.Length == 2).
					ToDictionary(parts => int.Parse(parts[0]), parts => parts[1]);
			var medicalGuide = File.ReadAllLines(tableFileName).Select(s => s.Trim().Split('\t')).Where(parts => parts.Length == 3).ToDictionary(
				parts => Tuple.Create(deses[int.Parse(parts[0])], meds[int.Parse(parts[1])]), parts => int.Parse(parts[2]));
			return new DeseasesToMedicamentsTable(medicalGuide);
		}
	}

	[TestFixture]
	public class TestTable
	{
		[Test, Explicit("Таблица")]
		public void GetTables()
		{
			const int minAmount = 30;
			//var deseases = Deseases.GetDefaultIndex().ToArray();
			var deseases = Deseases.GetFuzzyIndex().ToArray();
			//var medicaments = Medicaments.GetDefaultIndex().ToArray();
			var medicaments = Medicaments.GetFuzzyIndex().ToArray();
			var deseasesFromAnswers = Deseases.GetIndexFromAnswers().ToArray();
			var symptoms = Symptoms.GetDefaultIndex().Where(a => a.Ids.Count > minAmount).ToArray();

			var q = new DeseasesToMedicamentsTable(deseases, medicaments, 1);
			File.WriteAllText(Program.FilesDirectory + "deseases-medicaments.txt", q.ToStringOneToMany(10));

			var w = new DeseasesToMedicamentsTable(symptoms, medicaments, 10);
			File.WriteAllText(Program.FilesDirectory + "symptoms-medicaments.txt", w.ToString());

			var e = new DeseasesToMedicamentsTable(symptoms, deseasesFromAnswers, 10);
			File.WriteAllText(Program.FilesDirectory + "symptoms-deseases.txt", e.CapitulatoryStringByFirst());
		}

		[Test, Explicit("Таблица 'Заболевание - Медикамент' по реестру МКБ-10")]
		public static void GenerateMkb10DeseasesMedicamentsTable()
		{
			var parser = new Mkb10PageParser(Program.FilesDirectory + "Mkb10/");
			var deseases = parser.GetMkb10DeseasesFuzzyIndex().ToArray();
			var medicaments = parser.GetMkb10MedicamentsFuzzyIndex().ToArray();
			//				File.WriteAllLines();
			var table = new DeseasesToMedicamentsTable(deseases, medicaments, 1);
			File.WriteAllText("Mkb10-deseases-medicaments.txt", table.ToStringOneToMany(30));
			table.ExportTo("Mkb10-Deseases-Medicaments-Table-Exported.txt", "Mkb10-Deseases-Exported.txt", "Mkb10-Medicaments-Exported.txt");
		}

		[Test, Explicit]
		public void TestFuzzyDeseaseMedicamentQuestionsCount()
		{
			var deseaseMedicamentQuestions = new List<long>();

			var medicaments = Medicaments.GetDefaultIndex().ToArray();
			var deseases = Deseases.GetDefaultIndex().ToArray();

			foreach (var desMedQuests in from desease in deseases from medicament in medicaments select desease.Ids.Intersect(medicament.Ids).ToList())
				deseaseMedicamentQuestions.AddRange(desMedQuests);
			Console.WriteLine("Desease - medicament question pairs: " + deseaseMedicamentQuestions.Distinct().Count());
		}

		[Test, Explicit]
		public void GetFuzzyDeseaseNotMedicamentQuestions()
		{
			var medicamentInAnswersQuestionIds = new HashSet<long>(Medicaments.GetFuzzyIndex().SelectMany(med => med.Ids));
			var qs = new List<string>();
			foreach (var desease in Deseases.GetFuzzyIndex())
			{
				var des = desease;
				qs.AddRange(desease.Ids.Except(medicamentInAnswersQuestionIds).Select(
					id => string.Format("{0}\n{1}\n{2}\n---\n{3}\n===\n", des.Word, id, Program.DefaultNotStemmedQuestionList.GetQuestion(id).WholeText, String.Join("\n", Program.DefaultNotStemmedQuestionList.GetQuestion(id).GetAnswers().Select(a => a.Text)))));
			}
			File.WriteAllLines("Deseases_not_medicaments.txt", qs);
		}

		[Test, Explicit]
		public void GetQuestionDeseasAnswerDeseaseItems()
		{
			var qstDesIndex = Deseases.GetFuzzyIndex();
			var ansDesIndex = Deseases.GetIndexFromAnswers().ToList();
			var qs = new Dictionary<Tuple<string, string>, int>();
			foreach (var qstDes in qstDesIndex)
			{
				var des = qstDes;
				foreach (var ansDes in ansDesIndex.Where(ansDes => des.Word != ansDes.Word))
				{
					qs[Tuple.Create(qstDes.Word, ansDes.Word)] = qstDes.Ids.Intersect(ansDes.Ids).Count();
				}
			}
			var desVectors = new Dictionary<Tuple<string, string>, int>();
			foreach (var pair in qs)
			{
				var reversed = Tuple.Create(pair.Key.Item2, pair.Key.Item1);
				if (desVectors.ContainsKey(pair.Key) || desVectors.ContainsKey(reversed))
					continue;
				if (!qs.ContainsKey(reversed))
				{
					desVectors[pair.Key] = pair.Value;
					continue;
				}
				desVectors[pair.Key] = qs[reversed] + qs[pair.Key];
				//					if (qs[reversed] > pair.Value)
				//						desVectors[reversed] = qs[reversed] - pair.Value;
				//					else
				//						desVectors[pair.Key] = pair.Value - qs[reversed];
			}
			File.WriteAllText("DeseaseQstDeseaseAnsFreqs.gv",
							   "graph DeseasesRelations {\n" +
							   String.Join("\n",
										   desVectors.Where(it => it.Value >= 30).OrderByDescending(it => it.Value).Select(
											it => "\t" + it.Key.Item1 + "->" + it.Key.Item2 + "[weight=" + it.Value + "];"))
											+ "\n}");
		}
		
		[Test, Explicit("Таблица 'Заболевание - Медикамент' из обработанных вручную файлов")]
		public static void GenerateMkb10DeseasesMedicamentsTableFromFiles()
		{
			const string deseasesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseases.txt";
			const string deseaseNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseaseNames.txt";
			const string medicamentNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-medicamentNames.txt";

			var mkb10Deseases =
				Mkb10PageParser.GetStemmedDeseases(
					Mkb10PageParser.GetDeseasesFrom(deseasesFileName, deseaseNamesFileName, medicamentNamesFileName),
					Program.FilesDirectory + "Mkb10/").ToList();

			var deseases = Mkb10PageParser.GetMkb10DeseasesFuzzyIndex(mkb10Deseases).ToArray();
			var medicaments = Mkb10PageParser.GetMkb10MedicamentsFuzzyIndex(mkb10Deseases).ToArray();
			//				File.WriteAllLines();
			var table = new DeseasesToMedicamentsTable(deseases, medicaments, 1);
			File.WriteAllText("Mkb10-deseases-medicaments-from-test-serialized.txt", table.ToStringOneToMany(30));
		}
	}
}
