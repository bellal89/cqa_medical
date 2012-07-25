using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;
using cqa_medical.Utilits;

namespace cqa_medical.Statistics
{
	internal class DeseasesToMedicamentsTable
	{
		public Dictionary<string, Dictionary<string, int>> MedicalGuide;
		private QuestionList questionList;
		private readonly InvertedIndexUnit[] deseases;
		private readonly InvertedIndexUnit[] medicaments;

		public DeseasesToMedicamentsTable(QuestionList questionList, InvertedIndexUnit[] deseases,
		                                  InvertedIndexUnit[] medicaments)
		{
			MedicalGuide = new Dictionary<string, Dictionary<string, int>>();
			this.questionList = questionList;
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
			if (howMany == 0) return;
			if (!MedicalGuide.ContainsKey((desease)))
				MedicalGuide.Add(desease, new Dictionary<string, int>());
			MedicalGuide[desease].UpdateOrAdd(medicament, v => v + howMany, 1);
		}
	}

}
