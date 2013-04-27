using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LinqLib.Sequence;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.SpellChecker
{
	public class FuzzyIndex
	{
		private readonly Dictionary<string, int> wordFrequencies = new Dictionary<string, int>();
		private readonly HashSet<string> stopWords;
		private readonly TrigramIndex trigramIndex;

		private readonly List<InvertedIndexUnit> index;
		private readonly Dictionary<Tuple<string, string>, int> misspellingsIndex;
		private readonly Dictionary<string, string> fuzzyDictionary;
		private readonly Dictionary<string, HashSet<long>> termToIds = new Dictionary<string, HashSet<long>>();


		private readonly Dictionary<Tuple<string, string>, List<Tuple<string, string>>> misspellingsToWords = new Dictionary<Tuple<string, string>, List<Tuple<string, string>>>(); 

		public FuzzyIndex(IEnumerable<Tuple<long, string>> idTextList, IEnumerable<string> words)
		{
			trigramIndex = new TrigramIndex(words);

			File.WriteAllLines("TrigramIndex.txt", trigramIndex.Trigrams.OrderByDescending(t => t.Value.Count).Select(t => t.Key + "\t" + String.Join(", ", t.Value.Select(id => trigramIndex.IdToWord[id]))));

			var idWordsList = idTextList.Select(idText => Tuple.Create(idText.Item1,
			                                                           idText.Item2.SplitInWordsAndStripHTML())).ToList();
			FillWordFrequencies(idWordsList);
			stopWords = new HashSet<string>(wordFrequencies.OrderByDescending(kv => kv.Value).Take(130).Select(kv => kv.Key));

			idWordsList = idWordsList.Select(idWords =>
					Tuple.Create(
						idWords.Item1, 
						idWords.Item2.Where(w => !stopWords.Contains(w) && w.Length >= 3).Distinct())
					).ToList();

			var unknownWords =
				idWordsList.SelectMany(idWords => idWords.Item2).Distinct().Where(w => !trigramIndex.ContainsWord(w)).ToList();

			var levensteinInfos = RetrieveLevensteinInfos(unknownWords);
			misspellingsIndex = GetMisspellingsIndex(levensteinInfos).ToDictionary(kv => kv.Key, kv => kv.Value);
			fuzzyDictionary = GetFuzzyDictionary(levensteinInfos);

			foreach (var idWords in idWordsList)
			{
				foreach (var word in idWords.Item2)
				{
					var dictionaryWord = trigramIndex.ContainsWord(word)
					                     	? word
					                     	: (fuzzyDictionary.ContainsKey(word) 
												? fuzzyDictionary[word] 
												: null);
					if(dictionaryWord == null) continue;
					if (!termToIds.ContainsKey(dictionaryWord))
						termToIds[dictionaryWord] = new HashSet<long>();
					termToIds[dictionaryWord].Add(idWords.Item1);
				}
			}
			index = termToIds.Select(termId => new InvertedIndexUnit(termId.Key, termId.Value)).ToList();

			File.WriteAllLines("__Misspellings_To_Words.txt", misspellingsToWords.OrderByDescending(mw => mw.Value.Count).Select(mw => mw.Key.Item1 + "\t" + mw.Key.Item2 + "\t" + mw.Value.Count + "\t" + String.Join(", ", mw.Value)));
		}

		public IEnumerable<InvertedIndexUnit> GetIndex()
		{
			return index;
		}

		private void FillWordFrequencies(IEnumerable<Tuple<long, IEnumerable<string>>> idTextList)
		{
			foreach (var word in idTextList.SelectMany(idText => idText.Item2))
			{
				if (!wordFrequencies.ContainsKey(word))
					wordFrequencies[word] = 0;
				wordFrequencies[word]++;
			}
		}
		
		private Dictionary<string, string> GetFuzzyDictionary(IEnumerable<LevensteinInfo> levensteinInfos)
		{
			const int mispellRatio = 27;
			levensteinInfos = levensteinInfos.ToList();
			//File.WriteAllLines("__Dictionary_words_to_variants.txt", levensteinInfos.GroupBy(info => info.GetDictionaryWord(), (key, infos) => new { dictionaryWord = key, infos }).OrderByDescending(it => wordFreqs.ContainsKey(it.dictionaryWord) ? wordFreqs[it.dictionaryWord] : 0).Select(it => it.dictionaryWord + "\t" + String.Join(", ", it.infos.OrderByDescending(i => wordFreqs.ContainsKey(i.GetWord()) ? wordFreqs[i.GetWord()] : 0).Select(i => i.GetWord() + " (" + (wordFreqs.ContainsKey(i.GetWord()) ? wordFreqs[i.GetWord()] : 0) + ")"))));
			//File.WriteAllLines("__Variants_to_dictionary_words.txt", levensteinInfos.GroupBy(info => info.GetWord(), (key, infos) => new { word = key, infos }).OrderByDescending(it => it.infos.Count()).Select(it => it.word + "\t" + String.Join(", ", it.infos.OrderByDescending(i => wordFrequencies.ContainsKey(i.GetDictionaryWord()) ? wordFrequencies[i.GetDictionaryWord()] : 0).Select(i => i.GetDictionaryWord() + " (" + (wordFrequencies.ContainsKey(i.GetDictionaryWord()) ? wordFrequencies[i.GetDictionaryWord()] : 0) + ")"))));

			for (int k = 0; k < 5; k++)
			{
				Console.WriteLine(k + ": " + (double)levensteinInfos.Count(i => i.GetDistance() == k) / levensteinInfos.Count());
			}

			var levensteinGroups = levensteinInfos
				.Where(info =>
				       // I think most frequent words is correct.
				       // Clean levensteinInfos from those which contain most frequent words except the word itself.
				       info.GetDictionaryWord() == info.GetWord() ||
				       wordFrequencies.ContainsKey(info.GetWord()) &&
				       wordFrequencies.ContainsKey(info.GetDictionaryWord()) &&
				       wordFrequencies[info.GetWord()]*mispellRatio <= wordFrequencies[info.GetDictionaryWord()])
				.GroupBy(info => info.GetWord(),
				         (key, infos) => new
				                         	{
				                         		word = key,
				                         		infos =
				                         	infos.OrderBy(
				                         		info =>
				                         		wordFrequencies.ContainsKey(info.GetDictionaryWord())
				                         			? wordFrequencies[info.GetDictionaryWord()]
				                         			: 0)
				                         	});
			return levensteinGroups.Select(
				g =>
				g.infos.ElementAtMax(
					info =>
						{
							var m = info.GetMisspelling();
							return misspellingsIndex.ContainsKey(m) ? misspellingsIndex[m] : 0;
						})).ToDictionary
				(info => info.GetWord(), info => info.GetDictionaryWord());
		}

		private List<LevensteinInfo> RetrieveLevensteinInfos(IEnumerable<string> words)
		{
			var results = words.SelectMany(w =>
			{
				var editDistance = 2;
				if (w.Length < 10) editDistance = 1;
				if (w.Length < 5) editDistance = 0;
				return FindClosestWords(w, editDistance);
			}).ToList();
			return results;
		}

		private IEnumerable<LevensteinInfo> FindClosestWords(string word, int editDistance)
		{
			var wordTrigrams = TrigramIndex.GetTrigramsFrom(word);
			return trigramIndex.GetWordListUnion(wordTrigrams).Select(
				dictionaryWord => new LevensteinInfo(dictionaryWord, word)).Where(info => info.GetDistance() <= editDistance);
		}

		private IEnumerable<KeyValuePair<Tuple<string, string>, int>> GetMisspellingsIndex(IEnumerable<LevensteinInfo> levensteinInfos)
		{
			return DataActualityChecker.Check(
				new Lazy<KeyValuePair<Tuple<string, string>, int>[]>(
				() =>
				{
					// fuzzyTerm = (string misspelledWord, List<string> dictWordList)
					var missIndex = new Dictionary<Tuple<string, string>, int>();
					foreach (var info in levensteinInfos.Where(info => info.GetDistance() == 1))
					{
						AddMisspellingsTo(missIndex, info);
						AddMisspellingsWords(info);
					}
					return missIndex.OrderByDescending(kv => kv.Value).ToArray();
				}),
			FormatStringWrite,
			FormatStringParse,
			new FileDependencies(
				Program.FilesDirectory + "MisspellingsIndex.txt",
				Program.DeseasesFileName));
		}

		private void AddMisspellingsWords(LevensteinInfo info)
		{
			var misspelling = info.GetMisspelling();
			if (!misspellingsToWords.ContainsKey(misspelling))
				misspellingsToWords[misspelling] = new List<Tuple<string, string>>();
			misspellingsToWords[misspelling].Add(Tuple.Create(info.GetDictionaryWord(), info.GetWord()));
		}

		private static void AddMisspellingsTo(IDictionary<Tuple<string, string>, int> missIndex, LevensteinInfo info)
		{
			var misspelling = info.GetMisspelling();
			if (!missIndex.ContainsKey(misspelling))
				missIndex[misspelling] = 0;
			missIndex[misspelling]++;
		}

		private static KeyValuePair<Tuple<string, string>, int> FormatStringParse(string formattedString)
		{
			var q = formattedString.Split('\t');
			return new KeyValuePair<Tuple<string, string>, int>(Tuple.Create(q[0], q[1]), Int32.Parse(q[2]));
		}

		private static string FormatStringWrite(KeyValuePair<Tuple<string, string>, int> unit)
		{
			return unit.Key.Item1 + "\t" + unit.Key.Item2 + "\t" + unit.Value;
		}
	}
}
