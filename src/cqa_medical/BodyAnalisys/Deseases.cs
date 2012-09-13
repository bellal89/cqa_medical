using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MicrosoftResearch.Infer.Collections;
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
			const string neededUrlsFileName = @"..\..\BodyAnalisys\DeseasesUrls.txt";

			var words = new HashSet<string>();
			var urls = File.ReadAllLines(neededUrlsFileName).Where(q => !string.IsNullOrWhiteSpace(q));
			foreach(var url in urls)
			{
				var response = WebRequest.Create(url).GetResponse();
				using (var sr = (response.GetResponseStream()))
				{
					var bytes = sr.ReadAllBytes();
					var html = new HtmlDocument();
					var path = "1.html";
					File.WriteAllBytes(path, bytes);
					html.DetectEncodingAndLoad(path);
					var values = html.DocumentNode
						.Descendants()
						.Where(
							lnks =>
								lnks.ParentNode != null && lnks.ParentNode.ParentNode != null &&
							(lnks.ParentNode.ParentNode.Attributes["class"].Name == "sectiontableentry1" ||
							lnks.ParentNode.ParentNode.Attributes["class"].Name == "sectiontableentry2") &&
							lnks.Name == "a" &&
							lnks.Attributes["href"] != null &&
							lnks.InnerText.Trim().Length > 0)
						.Select(lnks => lnks.InnerText);
					words.AddRange( values.Distinct());
				}
			}

			Console.WriteLine(String.Join("\n", words));
			throw new NotImplementedException();
			
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
		public static IEnumerable<InvertedIndexUnit> GetIndexFromAnswers()
		{
			return DataActualityChecker.Check(
				new Lazy<InvertedIndexUnit[]>(
					() =>
						{
							var ql = Program.DefaultQuestionList;
							var des = GetFullDeseases(Program.DefaultMyStemmer);
							return des.GetIndex(ql
							                    	.GetAllAnswers()
							                    	.Select(t => Tuple.Create(t.QuestionId, t.Text))
								)
								.OrderByDescending(k => k.Ids.Count)
								.ToArray();
						}),
				InvertedIndexUnit.FormatStringWrite,
				InvertedIndexUnit.FormatStringParse,
				new FileDependencies(
					Program.FilesDirectory + "DeseasesIndexFromAnswers.txt",
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

		public static List<Uri> getLinks(HtmlDocument doc, string urlBase)
		{
			var linksOnPage = from lnks in doc.DocumentNode.Descendants()
							  where lnks.Name == "a" &&
								   lnks.Attributes["href"] != null &&
								   lnks.InnerText.Trim().Length > 0
							  select new
							  {
								  Url = lnks.Attributes["href"].Value,
							  };

			return linksOnPage.Select(link =>
			{
				Uri baseUri = new Uri(urlBase, UriKind.Absolute);
				Uri page = new Uri(baseUri, link.Url);
				return page;
			}).ToList();

		}
		[Test]
		public void GetNewUrls()
		{
			// проблема с русскими буквами
			//			var urlName = "http://www.neuronet.ru/bibliot/bme/menu.html";
			//			var urlName = "http://ru.wikipedia.org/wiki/%D0%9A%D0%B0%D1%82%D0%B5%D0%B3%D0%BE%D1%80%D0%B8%D1%8F:%D0%A1%D0%BF%D0%B8%D1%81%D0%BA%D0%B8:%D0%9C%D0%B5%D0%B4%D0%B8%D1%86%D0%B8%D0%BD%D0%B0";
			var urlName = "http://www.med-spravochnik.ru/bolezni/index.php";
			//			var urlName = "http://www.vidal.ru/patsientam/spisok-boleznei-po-alfavitu/";
			//			var urlName = "http://eistomin.narod.ru/context2.htm";
			//			var urlName = "http://mnashe.tripod.com/psych/luizahey.htm";
			//			var urlName = "http://www.dowlatow.ru/95.html";
			//			var urlName = "http://sicknesses.msk.ru/archives/category/%D0%B1%D0%BE%D0%BB%D0%B5%D0%B7%D0%BD%D0%B8";
			//			var urlName = "http://bono-esse.ru/blizzard/Socpom/ICD/mkb_1.html";
			//			var urlName = "http://www.diagnos-online.ru/symptoms.html";
			//			var urlName = "http://hmed.ru/abc171.html";

			//			синонимы и общие справочники
			//			var urlName = "http://www.webapteka.by/index.php?module=Simptomi&list=%C0";
			//			var urlName = "http://eistomin.narod.ru/glos.htm";
			//			var urlName = "http://medpage.su/";


			var response = WebRequest.Create(urlName).GetResponse();
			using (var sr = (response.GetResponseStream()))
			{
				var bytes = sr.ReadAllBytes();
				var html = new HtmlDocument();
				var path = "1.html";
				File.WriteAllBytes(path, bytes);
				html.DetectEncodingAndLoad(path);
				var values = getLinks(html, urlName);
				Console.WriteLine(String.Join("\n", values));
			}
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
