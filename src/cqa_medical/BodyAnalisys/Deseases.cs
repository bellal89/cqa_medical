using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.Utilits;

namespace cqa_medical.BodyAnalisys
{
	public class Deseases
	{
		public HashSet<string> DeseasesList { get; private set; }

		private Deseases(IStemmer stemmer, IEnumerable<string> deseases)
		{
			DeseasesList = new HashSet<string>(deseases.Select(stemmer.Stem));
		}

		public static Deseases GetFromDeseasesTxtFile(IStemmer stemmer)
		{
			return new Deseases(stemmer, GetDeseasesFromDeseasesTxtFile().ToList());
		}

		public static Deseases GetFromLazarevaManual(IStemmer stemmer)
		{
			return new Deseases(stemmer, GetDeseasesFromLazarevaManual().ToList());
		}
		public static Deseases GetFromInternet(IStemmer stemmer)
		{
			return new Deseases(stemmer, GetIndexFromInternetMedicalDictionary().ToList());
		}


		// очень завязан на файл Deseases.txt
		private static IEnumerable<string> GetDeseasesFromDeseasesTxtFile()
		{
			var tabulationParser = new TabulationParser();
			var neededWords =
				tabulationParser.ParseFromFile(Program.DeseasesFileName)
					.Skip(998)
					.Take(711)
					.Where(t => t.IndicatorAmount == 1)
					.ToList();

			var splittedWords = neededWords.SelectMany(s => s.Words.TakeWhile(r => r != "--")).ToArray();
			const string notDeseasesFileName = "BodyAnalisys/notDeseases.txt";
			var q = splittedWords.Where(t => !(
			                                  	t.Length < 3 ||
			                                  	Regex.IsMatch(t, @"[^йцукенгшшщзхъфывапролджэячсмитьбю]") ||
			                                  	Regex.IsMatch(t, @"(ый|ой|ая|ий)$") ||
			                                  	File.ReadAllLines(notDeseasesFileName).Contains(t)
			                                  )
				).ToArray();
			return q.Distinct().OrderBy(s => s);
		}
		private static IEnumerable<string> GetDeseasesFromLazarevaManual()
		{
			var text = File.ReadAllText(Program.LazarevaManualFileName);
			var regexp = new Regex(@"(?<x>[\w ]+?)\r\n\1");
			var matches = regexp.Matches(text);
			for (int i = 0; i < matches.Count; i++)
			{
				var deseaseString = matches[i].Groups["x"].Value;
				if (deseaseString.Length > 1)
					yield return deseaseString;
			}
		}
		private static IEnumerable<string> GetIndexFromInternetMedicalDictionary()
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

		public static IEnumerable<InvertedIndexUnit> GetDefault()
		{
			return DataActualityChecker.Check(
				new Lazy<InvertedIndexUnit[]>(
					() =>
						{
							var ql = Program.DefaultQuestionList;
							var des = GetFromDeseasesTxtFile(Program.DefaultMyStemmer);
							return des.GetIndex(ql
							                    	.GetAllQuestions()
							                    	.Select(t => Tuple.Create(t.Id, t.WholeText)))
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
	public class GetDeseases
	{
		[Test, Explicit]
		public void GetRight()
		{
			var q  = Deseases.GetDefault().OrderByDescending(qw => qw.Ids.Count());
			File.WriteAllLines("rtyy.txt", q.Select(s=> s.Word +"\t" +s.Ids.Count()));
		}
		[Test, Explicit]
		public void Get()
		{
			var ql = Program.DefaultQuestionList;
			var des = Deseases.GetFromDeseasesTxtFile(Program.DefaultMyStemmer);
			var deseasesIndex =
				des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText)));
			File.WriteAllLines("DeseasesIndex.txt", deseasesIndex.Select(s => s.ToString()));
		}
		[Test]
		public void GetFromLazarevaManual()
		{
			var ql = Program.DefaultQuestionList;
			var des = Deseases.GetFromLazarevaManual(Program.DefaultMyStemmer);
			var ans = des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText))).ToArray();
			var result = ans.OrderByDescending(qw => qw.Ids.Count());
			File.WriteAllLines("dfsg.txt", result.Select(s => s.Word + "\t" + s.Ids.Count()));
		}

		[Test]
		public void TestGetIndexFromInternetMedicalDictionary()
		{
			var des = Deseases.GetFromInternet(Program.DefaultMyStemmer);
		}
	}
}
