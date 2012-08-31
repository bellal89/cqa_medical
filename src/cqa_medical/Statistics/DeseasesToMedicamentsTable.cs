using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.BodyAnalisys;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Statistics
{
	public class DeseasesToMedicamentsTable
	{
		public Dictionary<Tuple<string,string>,  int> MedicalGuide;
		private readonly InvertedIndexUnit[] deseases;
		private readonly InvertedIndexUnit[] medicaments;
		private readonly int minAmount;

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



		[TestFixture]
		public class TestTable
		{
			[Test, Explicit("Таблица")]
			public void GetTables()
			{
				const int minAmount = 30;
				var medicaments = Medicaments.GetDefaultIndex().ToArray();
				var deseases = Deseases.GetDefaultIndex().ToArray();
				var deseasesFromAnswers = Deseases.GetIndexFromAnswers().ToArray();
				var symptoms = Symptoms.GetDefaultIndex().Where(a => a.Ids.Count > minAmount).ToArray();
				var q = new DeseasesToMedicamentsTable(deseases, medicaments, 1);
				File.WriteAllText("../../Files/deseases-medicaments.txt", q.ToString());
				var w = new DeseasesToMedicamentsTable(symptoms, medicaments,10);
				File.WriteAllText("../../Files/symptoms-medicaments.txt", w.ToString());
				var e = new DeseasesToMedicamentsTable(symptoms, deseasesFromAnswers, 10);
				File.WriteAllText("../../Files/symptoms-deseases.txt", e.CapitulatoryStringByFirst());
			}
		}

	}

}
