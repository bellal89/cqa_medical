using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.BodyAnalisys
{
	class Symptoms
	{
		private readonly Vocabulary vocabulary;
		private readonly HashSet<string> mainWords;
		private readonly string neededPartOfSpeech;

		private readonly int radius;

		public Symptoms(Vocabulary vocabulary, IEnumerable<string> mainWords, string neededPartOfSpeech, int radius = 2)
		{
			this.vocabulary = vocabulary;
			this.neededPartOfSpeech = neededPartOfSpeech;
			this.radius = radius;
			this.mainWords = new HashSet<string>(mainWords);
		}

		public List<InvertedIndexUnit> GetSymptomsIndex(IEnumerable<Tuple<long, string>> idAndTextList)
		{
			var symptomToQuestionList = new Dictionary<string, List<long>>();

			foreach (var pair in idAndTextList)
			{
				var words = pair.Item2.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
				for (var i = 0; i < words.Length; i++)
				{
					if (!mainWords.Contains(words[i])) continue;
					var verbs = FilterWords(words, i).ToArray();
					foreach (var symptom in verbs.Where(verb => words[i] != verb))
					{
						var formattedSymptom = words[i] + "_" + symptom;
						if (!symptomToQuestionList.ContainsKey(formattedSymptom))
							symptomToQuestionList.Add(formattedSymptom, new List<long>());
						symptomToQuestionList[formattedSymptom].Add(pair.Item1);
					}
				}
			}

			return symptomToQuestionList.Select(item => new InvertedIndexUnit(item.Key, item.Value)).ToList();
		}
		public List<InvertedIndexUnit> GetSymptomsIndex(IEnumerable<Tuple<long, string>> idAndTextList, HashSet<string> expectedWords)
		{
			var symptomToQuestionList = new Dictionary<string, List<long>>();

			foreach (var pair in idAndTextList)
			{
				var words = pair.Item2.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
				for (var i = 0; i < words.Length; i++)
				{
					if (!mainWords.Contains(words[i])) continue;
					var verbs = FilterWords(words, i).ToArray();
					foreach (var symptom in verbs.Where(verb => words[i] != verb && expectedWords.Contains(verb)))
					{
						var formattedSymptom = words[i] + "_" + symptom;
						if (!symptomToQuestionList.ContainsKey(formattedSymptom))
							symptomToQuestionList.Add(formattedSymptom, new List<long>());
						symptomToQuestionList[formattedSymptom].Add(pair.Item1);
					}
				}
			}

			return symptomToQuestionList.Select(item => new InvertedIndexUnit(item.Key, item.Value)).ToList();
		}
	

		private IEnumerable<string> FilterWords(IList<string> words, int pos)
		{
			var minPos = pos - radius >= 0 ? pos - radius : 0;
			var maxPos = pos + radius < words.Count ? pos + radius : words.Count - 1;
			for (var i = minPos; i <= maxPos; i++)
			{
				var partOfSpeech = vocabulary.GetPartOfSpeech(words[i]);
				if (partOfSpeech != null && partOfSpeech == neededPartOfSpeech)
				{
					yield return words[i];
				}
			}
		}

		public static IEnumerable<InvertedIndexUnit> GetDefaultIndex()
		{

			return DataActualityChecker.Check(
				new Lazy<InvertedIndexUnit[]>(
					() =>
						{
							var body = BodyPart.GetBodyPartsFromFile(Program.BodyPartsFileName);
							var searcher = new Symptoms(Program.DefaultVocabulary, body.GetWords(), "V");
							var questionList = Program.DefaultQuestionList;
							return searcher.GetSymptomsIndex(questionList
							                            	.GetAllQuestions()
							                            	.Select(item => Tuple.Create(item.Id, item.WholeText)))
								.OrderByDescending(k => k.Ids.Count)
								.ToArray();
						}),
				InvertedIndexUnit.FormatStringWrite,
				InvertedIndexUnit.FormatStringParse,
				new FileDependencies(
					Program.SymptomsIndexFileName,
					Program.QuestionsFileName));
		}
	}

	[TestFixture]
	public class SymptomSearchTest
	{
		private const string NounsFileName = "nouns.txt";
		private const string VerbsFileName = "verbs.txt";

		[Test, Explicit]
		public static void TestSearch()
		{
			var voc = Program.DefaultVocabulary;
			var body = BodyPart.GetBodyPartsFromFile(Program.BodyPartsFileName);
			var searcher = new Symptoms(voc, body.GetWords(), "V");

			var questionList = Program.TestDefaultQuestionList;

			var start = DateTime.Now;
			var symptoms = searcher
				.GetSymptomsIndex(questionList.GetAllQuestions().Select(item => Tuple.Create(item.Id, item.WholeText)));
			Console.WriteLine("Symptoms found at {0} seconds.", (DateTime.Now - start).TotalSeconds);
			Console.WriteLine(String.Join("\n",symptoms.Select(s => s.ToString())));
		}
		

		[Test, Explicit]
		public static void NounsSearch()
		{
			var voc = Program.DefaultVocabulary;
			var mainWords = voc.GetAllInfos().Where(s => s.PartOfSpeach == "V").Select(s=> s.Stem);
			var searcher = new Symptoms(voc, mainWords, "S");

			var questionList = Program.DefaultQuestionList;

			var symptoms = new Func<List<InvertedIndexUnit>>(()=> searcher
				.GetSymptomsIndex(questionList.GetAllQuestions().Select(item => Tuple.Create(item.Id, item.WholeText)))
				.ToList()).DetectTime("nouns found");
			File.WriteAllLines("nouns_verbs.txt", symptoms.Select(s => s.ToStringCount()));
		}

		[Test, Explicit]
		public static void GetSymptoms()
		{
			var start = DateTime.Now;
			var symptoms = Symptoms.GetDefaultIndex().ToList();
			Console.WriteLine("Symptoms found at {0} seconds.", (DateTime.Now - start).TotalSeconds);
			File.WriteAllLines("SymptomIndex.txt", symptoms.Select(s => s.ToString()));
			File.WriteAllLines("SymptomIndexCount.txt", symptoms.Select(s => s.ToStringCount()));
		}
		public SortedDictionary<string, int> WordsFrequensy(HashSet<string> words, IEnumerable<string> allWords )
		{
			return new DistributionCreator<string>(allWords.Where(words.Contains)).GetData();
		}
		[Test, Explicit]
		public void NounsAndVerbs()
		{
			var voc = Program.DefaultVocabulary;
			var verbs = voc.GetAllInfos().Where(s => s.PartOfSpeach == "V").Select(s => s.Stem);
			var nouns = voc.GetAllInfos().Where(s => s.PartOfSpeach == "S").Select(s => s.Stem);
			var ql = Program.DefaultQuestionList;
			var questionWords = ql.GetAllQuestions().SelectMany(q => q.WholeText.SplitInWordsAndStripHTML());
			var answerswords = ql.GetAllAnswers().SelectMany(a => a.Text.SplitInWordsAndStripHTML());
			var allWords = questionWords.Concat(answerswords);

			var verbFreq = new Func<SortedDictionary<string, int>>(
				() => WordsFrequensy(new HashSet<string>(verbs), allWords)).DetectTime("Verbs");
			File.WriteAllText(VerbsFileName, verbFreq.ToStringSortedByValue());
			var nounFreq = new Func<SortedDictionary<string, int>>(
				() => WordsFrequensy(new HashSet<string>(nouns), allWords)).DetectTime("Nouns");
			File.WriteAllText(NounsFileName, nounFreq.ToStringSortedByValue());
		}


 		private IEnumerable<string> GetWords(string filename)
 		{
			return File.ReadAllLines(filename)
 				.Select(
 					s =>
 						{
 							var a = s.Split(new[] {'\t', ' '}).ToArray();
 							var w = a[0].Split(new[] {'_'});
 							var word = (w.Count() == 2) ? w[1] : a[0];
 							return new Tuple<string, int>(word, int.Parse(a[1]));
 						}
 				)
				.Select(a => a.Item1);
 		}

		[Test, Explicit]
		public void GetNounsForVerbs()
		{
			var ql = Program.DefaultQuestionList;
			
			var questionWords = ql.GetAllQuestions().Select(item => Tuple.Create(item.Id, item.WholeText));
			var answerswords = ql.GetAllAnswers().Select(item => Tuple.Create(item.Id, item.Text));
			var allWords = questionWords.Concat(answerswords);

			var nouns = GetWords(NounsFileName).ToArray();
			var verbs = GetWords(VerbsFileName).ToArray();


			var voc = Program.DefaultVocabulary;
			var searcher = new Symptoms(voc, verbs, "S");
			var nounVerbIndex = searcher.GetSymptomsIndex(allWords, new HashSet<string>(nouns));
			File.WriteAllLines("nounVerbIndex.txt", nounVerbIndex.Select(InvertedIndexUnit.FormatStringWrite));
			File.WriteAllLines("nounVerbIndexCount.txt", nounVerbIndex.OrderByDescending(a => a.Ids.Count).Select(a => a.ToStringCount()));
		} 
		[Test, Explicit]
		public void Filter()
		{
			const string expectedWord = "болеть";
			var words = File.ReadAllLines("nounVerbIndexCount.txt");
			foreach (var word in words.Where(a => a.Split(new[]{'_'})[0] == expectedWord))
			{
				Console.WriteLine( word);
			}


		}
	}
}
