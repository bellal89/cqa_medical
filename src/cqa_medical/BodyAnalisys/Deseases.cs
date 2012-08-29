using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.BodyAnalisys
{
	public class Deseases
	{
		public HashSet<string> DeseasesList { get; private set; }

		private Deseases(IStemmer stemmer, IEnumerable<string> deseases)
		{
			DeseasesList = new HashSet<string>(CleanDeseases(stemmer, deseases));
		}
		public static Deseases GetFromDeseasesTxtFile(IStemmer stemmer)
		{
			return new Deseases(stemmer, GetCandidatesFromDeseasesTxtFile(stemmer).ToList());
		}

		public static Deseases GetFromLazarevaManual(IStemmer stemmer)
		{
			return new Deseases(stemmer, GetCandidatesFromLazarevaManual().ToList());
		}
		public static Deseases GetFromInternet(IStemmer stemmer)
		{
			return new Deseases(stemmer, GetIndexFromInternetMedicalDictionary(stemmer).ToList());
		}
		public static Deseases GetFullDeseases(IStemmer stemmer)
		{
			return new Deseases(
				stemmer,
				GetCandidatesFromDeseasesTxtFile(stemmer)
					.Concat(GetCandidatesFromLazarevaManual()).ToList());
		}

		private static IEnumerable<string> CleanDeseases(IStemmer stemmer, IEnumerable<string> candidateDeseases)
		{
			const string notDeseasesFileName = "../../BodyAnalisys/notDeseases.txt";
			var nonDeseasesWords = File.ReadAllLines(notDeseasesFileName);
			var notDeseases = new HashSet<string>(nonDeseasesWords.Select(stemmer.Stem).Concat(nonDeseasesWords));
			var russianWordRedex = new Regex(@"[^йцукенгшшщзхъфывапролджэячсмитьбю]");
			var adjectiveWordRegex = new Regex(@"(ый|ой|ая|ий|ого)$");
			var laboratoryMethodWordRegex = new Regex(@"(скопия|графия|лечение)$");

			var deseases = candidateDeseases
				.Select(stemmer.Stem)
				.Where(t => !(
								t.Length < 3 ||
								russianWordRedex.IsMatch(t) ||
								adjectiveWordRegex.IsMatch(t) ||
								laboratoryMethodWordRegex.IsMatch(t) ||
								notDeseases.Contains(t)
							 ));
			return deseases.Distinct();
		}


		// очень завязан на файл Deseases.txt
		private static IEnumerable<string> GetCandidatesFromDeseasesTxtFile(IStemmer stemmer)
		{
			var tabulationParser = new TabulationParser();
			var neededWords =
				tabulationParser.ParseFromFile(Program.DeseasesFileName)
					.Skip(998)
					.Take(711)
					.Where(t => t.IndicatorAmount == 1)
					.ToList();
			var splittedWords = neededWords.SelectMany(s => s.Words.TakeWhile(r => r != "--")).ToArray();

			return splittedWords.OrderBy(s => s);
		}
		private static IEnumerable<string> GetCandidatesFromLazarevaManual()
		{
			var text = File.ReadAllText(Program.LazarevaManualFileName);
			var regexp = new Regex(@"(?<x>[\w ]+?)\r\n\1");
			var matches = regexp.Matches(text);
			for (int i = 0; i < matches.Count; i++)
			{
				var deseaseString = matches[i].Groups["x"].Value;
				yield return deseaseString;
			}
		}
		private static IEnumerable<string> GetIndexFromInternetMedicalDictionary(IStemmer stemmer)
		{
			// not working yet
			// проблема с русскими буквами
			var urlName = "http://www.neuronet.ru/bibliot/bme/menu.html";
			//			var urlName = "http://ru.wikipedia.org/wiki/%D0%9A%D0%B0%D1%82%D0%B5%D0%B3%D0%BE%D1%80%D0%B8%D1%8F:%D0%A1%D0%BF%D0%B8%D1%81%D0%BA%D0%B8:%D0%9C%D0%B5%D0%B4%D0%B8%D1%86%D0%B8%D0%BD%D0%B0";
			//			var urlName = "http://www.med-spravochnik.ru/bolezni/index.php";
			//			var urlName = "http://www.vidal.ru/patsientam/spisok-boleznei-po-alfavitu/";

			var response = WebRequest.Create(urlName).GetResponse();
			using (StreamReader sr = new StreamReader(response.GetResponseStream()))
			{
				var text = sr.ReadToEnd();
				Console.Write(text);
				File.WriteAllText("1.html", text);
			}

			return null;
		}
		
		public IEnumerable<InvertedIndexUnit> GetIndex(IEnumerable<Tuple<long, string>> idTextList)
		{
			var deseasesToIds = new Dictionary<string, HashSet<long>>();
			foreach (var idAndText in idTextList)
			{
				var words = idAndText.Item2.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var word in words.Where(word => DeseasesList.Contains(word)))
				{
					if (!deseasesToIds.ContainsKey(word))
						deseasesToIds.Add(word, new HashSet<long>());
					deseasesToIds[word].Add(idAndText.Item1);
				}
			}
			return deseasesToIds.Select(item => new InvertedIndexUnit(item.Key, item.Value)).ToList();

		}

		public static IEnumerable<InvertedIndexUnit> GetDefaultIndex()
		{
			return DataActualityChecker.Check(
				new Lazy<InvertedIndexUnit[]>(
					() =>
						{
							var ql = Program.DefaultQuestionList;
							var des = GetFullDeseases(Program.DefaultMyStemmer);
							return des.GetIndex(ql
							                    	.GetAllQuestions()
							                    	.Select(t => Tuple.Create(t.Id, t.WholeText))
								)
								.OrderByDescending(k => k.Ids.Count)
								.ToArray();
						}),
				InvertedIndexUnit.FormatStringWrite,
				InvertedIndexUnit.FormatStringParse,
				new FileDependencies(
					Program.DeseasesIndexFileName,
					Program.DeseasesFileName));
		}
	}

	[TestFixture]
	public class GetDeseasesClass
	{
		[Test, Explicit]
		public void GetDeseases()
		{
			var q  = Deseases.GetFullDeseases(Program.DefaultMyStemmer).DeseasesList.OrderBy(s => s);
			File.WriteAllLines("deseases.txt", q);
		}
		[Test, Explicit]
		public void GetFromDeseasesTxt()
		{
			var ql = Program.DefaultQuestionList;
			var des = Deseases.GetFromDeseasesTxtFile(Program.DefaultMyStemmer);
			var deseasesIndex =
				des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText)));
			var result = deseasesIndex.OrderByDescending(qw => qw.Ids.Count());
			File.WriteAllLines("DeseasesIndex_FromDeseasesTxt.txt", result.Select(s => s.ToStringCount("\t")));
		}
		[Test,Explicit]
		public void GetFromLazarevaManual()
		{
			var ql = Program.DefaultQuestionList;
			var des = Deseases.GetFromLazarevaManual(Program.DefaultMyStemmer);
			var ans = des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText))).ToArray();
			var result = ans.OrderByDescending(qw => qw.Ids.Count());
			File.WriteAllLines("DeseasesIndex_FromLazarevaManual.txt", result.Select(s =>s.ToStringCount("\t")));
		}

		[Test]
		public void TestGetIndexFromInternetMedicalDictionary()
		{
			var des = Deseases.GetFromInternet(Program.DefaultMyStemmer);
		}

		[Test]
		public void JustNothing()
		{
			var q = File.ReadAllLines(@"C:\Users\kriskk\Documents\GitHub\cqa_medical\src\cqa_medical\BodyAnalisys\notDeseases.txt");
			File.WriteAllLines(@"C:\Users\kriskk\Documents\GitHub\cqa_medical\src\cqa_medical\BodyAnalisys\notDeseases.txt", q.Distinct());
		}
	}
}
