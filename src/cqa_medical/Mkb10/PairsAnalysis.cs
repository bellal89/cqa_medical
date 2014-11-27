using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.Mkb10
{
	class PairsAnalysis
	{
		private readonly Mkb10 mkb = new Mkb10();
		private readonly HashSet<Tuple<int, string>> correctSet;
		private readonly List<Tuple<int, string, List<long>>> extracted;
		private readonly List<Tuple<int, string, List<long>>> correctExtracted;

		public PairsAnalysis()
		{
			correctSet = new HashSet<Tuple<int, string>>(mkb.GetCorrectDesSubstancePairs());
			extracted = mkb.GetDesMedPairs();
			correctExtracted = extracted.Where(pair => correctSet.Contains(Tuple.Create(pair.Item1, pair.Item2))).ToList();
		}

		public HashSet<Tuple<int, string>> GetCorrect()
		{
			return correctSet;
		}

		public List<Tuple<int, string, List<long>>> GetExtraced()
		{
			return extracted;
		}

		public List<Tuple<int, string, List<long>>> GetCorrectExtracted()
		{
			return correctExtracted;
		}

		public Mkb10 GetMkb()
		{
			return mkb;
		}

		public IEnumerable<Tuple<int, string, List<long>, Question>> GetIncorrectPairQuestions (int minFrequency)
		{
			var nonCorrect = GetNonCorrectPairs(minFrequency).OrderByDescending(it => it.Item3);

			var desIndex = mkb.GetDeseasesFuzzyIndex().ToDictionary(it => it.Item1, it => it.Item2);
			var medIndex = mkb.GetDesActiveSubIndex().ToDictionary(it => it.Item1, it => it.Item2);

			var questions = Program.DefaultNotStemmedQuestionList;

			return nonCorrect.Select(
				it => Tuple.Create(it.Item1, it.Item2, it.Item3, desIndex[it.Item1].Ids.Intersect(medIndex[it.Item2].Ids))).
				SelectMany(
					it => it.Item4.Select(id => Tuple.Create(it.Item1, it.Item2, it.Item3, questions.GetQuestion(id))));
		}

		public IEnumerable<Tuple<int, string, List<long>>> GetNonCorrectPairs(int minFrequency)
		{
			var nonCorrect =
				extracted.Where(pair => !correctSet.Contains(Tuple.Create(pair.Item1, pair.Item2))).Where(
					pair => pair.Item3.Count >= minFrequency);
			return nonCorrect;
		}

		
	}

	[TestFixture]
	public class PairAnalysisExperiments
	{
		[Test]
		public void TestCreation()
		{
			var analysis = new PairsAnalysis();
			File.WriteAllText("PairsNonCorrectQuestions.txt", String.Join("\n---\n---\n",
			                                                              analysis.GetIncorrectPairQuestions(100).Select(
			                                                              	it =>
			                                                              	analysis.GetMkb().GetDesease(it.Item1).Name + "\n" +
			                                                              	it.Item2 + "\n" +
			                                                              	it.Item3 + "\n" + it.Item4.WholeText + "\n===\n" +
			                                                              	String.Join("\n",
			                                                              	            it.Item4.GetAnswers().Select(a => a.Text)))));
		}

		[Test]
		public void TestNonCorrectPairsFactCorrectness()
		{
			var analysis = new PairsAnalysis();
			var noncorrectSum = analysis.GetNonCorrectPairs(150).Sum(it => it.Item3.Count);
			var correctExtractedSum = (double)(analysis.GetCorrectExtracted().Sum(it => it.Item3.Count) + noncorrectSum);
			var correctExtractedCount = (double) (analysis.GetCorrectExtracted().Count + analysis.GetNonCorrectPairs(150).Count());
			
			Console.WriteLine("Correct extracted with handy tuning:" + correctExtractedCount/analysis.GetExtraced().Count);
			Console.WriteLine("Correct extracted sum:" + correctExtractedSum / analysis.GetExtraced().Sum(it => it.Item3.Count));
		}
	}
}
