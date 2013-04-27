using System;

namespace cqa_medical.DataInput
{
	[Serializable]
	public class Mkb10TradeName
	{
		public string Name { get; set; }
		public string ActiveSubstance { get; set; }
		public Mkb10TradeName(string name, string activeSubstance)
		{
			Name = name;
			ActiveSubstance = activeSubstance;
		}
	}
}