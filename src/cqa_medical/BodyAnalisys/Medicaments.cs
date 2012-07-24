using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace cqa_medical.BodyAnalisys
{
	class Medicaments
	{
		private Dictionary<string, List<int>> invertedIndex = new Dictionary<string, List<int>>(); 

		public Medicaments()
		{
			FromFile();
		}

		private void FromFile()
		{
			var i = 1;
			foreach (var line in File.ReadAllLines(Program.MedicamentsFileName))
			{
				var parts = line.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
				addToIndex(parts[0]);
				addToIndex(parts[1]);
			}
			
		}

		private void addToIndex(string p0)
		{
			throw new NotImplementedException();
		}
	}
}
