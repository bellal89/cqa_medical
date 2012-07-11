﻿using System;
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

		public DeseasesToMedicamentsTable( InvertedIndexUnit[] deseases, InvertedIndexUnit[] medicaments)
		{
			MedicalGuide = new Dictionary<Tuple<string, string>, int>();
			this.deseases = deseases;
			this.medicaments = medicaments;
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
			if (howMany < 50 ) return;
			MedicalGuide.Add(Tuple.Create(desease, medicament), howMany);
		}

		public override string ToString()
		{
			return String.Join("\n", MedicalGuide.OrderByDescending(a => a.Value).Select(t => String.Join("\t", t.Key.Item1 , t.Key.Item2 , t.Value)));
		}

		[TestFixture]
		public class TestTable
		{
			[Test, Explicit("Таблица")]
			public void GetTables()
			{
				const int minAmount = 50;
				var medicaments = Medicaments.GetDefaultIndex().Where(a => a.Ids.Count > minAmount).ToArray();
				var deseases = Deseases.GetDefaultIndex().Where(a => a.Ids.Count > minAmount).ToArray();
				var symptoms = Symptoms.GetDefaultIndex().Where(a => a.Ids.Count > minAmount).ToArray();
				var q = new DeseasesToMedicamentsTable(deseases, medicaments);
				File.WriteAllText("../../Files/deseases-medicaments.txt", q.ToString());
				var w = new DeseasesToMedicamentsTable(symptoms, medicaments);
				File.WriteAllText("../../Files/symptoms-medicaments.txt", w.ToString());
			}
		}

	}

}
