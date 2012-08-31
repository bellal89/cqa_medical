using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers.AOTLemmatizer;

namespace cqa_medical.BodyAnalisys
{
	class SymptomsFromFeldsher
	{
		public HashSet<string> SymptomsList { get; private set; }


		public SymptomsFromFeldsher()
		{
			SymptomsList = new HashSet<string>(InitialSymptoms());
		}

		public SymptomsFromFeldsher(IEnumerable<string> symptomsList)
		{
			SymptomsList = new HashSet<string>(symptomsList);
		}


		private IEnumerable<string> InitialSymptoms()
		{
			var keyWordsregex = new Regex(@"Основные клинические симптомы", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			var text = File.ReadAllText(Program.LazarevaManualFileName);
			var sentences = text.Split(new[] {'.'});
			return sentences.Where(s => keyWordsregex.IsMatch(s));
		}

		// форма 
		//		острая, тяжелая генерализованная, септическая
		// симптомы
		// характерно
		// характеризуется

	}

	[TestFixture]
	internal class Stest

	{
		[Test]
		public void GetInitialSymptoms()
		{
			var q = new SymptomsFromFeldsher();
			File.WriteAllLines("InitialSymptoms.txt", q.SymptomsList);
		}
		[Test]
		public void йцу()
		{
			var q = new AOTLemmatizer();
			var stemmer = Program.DefaultMyStemmer;

			File.WriteAllLines("InitialSymptoms.txt", q.GetAllForms("зуд").Select(s => stemmer.StemWithInfo(s).StemmedWord).Distinct());
		}
	}

}
