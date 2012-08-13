namespace cqa_medical.DataInput.Stemmers
{
	class EmptyStemmer : IStemmer
	{
		public string Stem(string word)
		{
			return word;
		}
	}
}
