using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.Utilits;

namespace cqa_medical.BodyAnalisys
{
	public class Deseases
	{
		public HashSet<string> DeseasesList;
		private readonly IStemmer stemmer;

		public Deseases(IStemmer stemmer)
		{
			this.stemmer = stemmer;
			DeseasesList = new HashSet<string>(GetDeseasesFromDeseasesTxtFile());
		}

		// очень завязан на файл Deseases.txt
		public IOrderedEnumerable<string> GetDeseasesFromDeseasesTxtFile()
		{
			var tabulationParser = new TabulationParser(stemmer);
			var neededWords =
				tabulationParser.ParseFromFile(Program.DeseasesFileName)
					.Skip(998)
					.Take(711)
					.Where(t => t.IndicatorAmount == 1)
					.ToList();

			var splittedWords = neededWords.SelectMany(s => s.StemmedWords.TakeWhile(r => r != "--")).ToArray();
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

		public IEnumerable<InvertedIndexUnit> GetIndex(IEnumerable<Tuple<long, string>> idTextList)
		{
			var deseasesToIds = new Dictionary<string, HashSet<long>>();
			foreach (var idAndText in idTextList)
			{
				var words = idAndText.Item2.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
				foreach (var word in words.Where(word => DeseasesList.Contains(word)))
				{
					if (!deseasesToIds.ContainsKey(word))
						deseasesToIds.Add(word, new HashSet<long>());
					deseasesToIds[word].Add(idAndText.Item1);
				}
			}
			return deseasesToIds.Select(item => new InvertedIndexUnit(item.Key, item.Value)).ToList();

		}

		public IEnumerable<string> GetIndexFromQuestionList(QuestionList ql)
		{
			var des = new Deseases(Program.DefaultMyStemmer);
			return des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText)))
				.Select(q => q.Word + " " + String.Join(" ", q.Ids));
		}

		public static IEnumerable<InvertedIndexUnit> GetDefault()
		{
			return DataActualityChecker.Check(
				new Lazy<InvertedIndexUnit[]>(() =>
				                              	{
				                              		var ql = Program.DefaultQuestionList;
				                              		var des = new Deseases(Program.DefaultMyStemmer);
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
		[Test]
		public void GetRight()
		{
			var q  = Deseases.GetDefault();
			File.WriteAllLines("rtyy.txt", q.Select(s=>s.ToString()));
		}
		[Test]
		public void Get()
		{
			var ql = Program.DefaultQuestionList;
			var des = new Deseases(Program.DefaultMyStemmer);
			var deseasesIndex =
				des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText)));
			File.WriteAllLines("DeseasesIndex.txt", deseasesIndex.Select(s => s.ToString()));
		}
	}
	[TestFixture]
	public class DeseasesTest
	{
		[Test]
		public void GetTest()
		{
			var ql = Program.TestDefaultQuestionList;
			var des = new Deseases(Program.DefaultMyStemmer);
			var deseasesIndex = des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText))).Select(q => q.Word + "_" + String.Join("+", q.Ids)).ToArray();
			Console.WriteLine(String.Join("\n",deseasesIndex));
			Assert.AreEqual(1,deseasesIndex.Length);
		}
	}
	
}
