using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.Utilits;

namespace cqa_medical.BodyAnalisys
{
	class Deseases
	{
		public string[] DeseasesList;
		private readonly IStemmer stemmer;
		public Deseases(IStemmer stemmer)
		{
			this.stemmer = stemmer;
			DeseasesList = GetDeseasesFromDeseasesTxtFile().ToArray();
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
			var q = splittedWords.Where(t => !(
												t.Length < 3 ||
			                                  	Regex.IsMatch(t, @"[^йцукенгшшщзхъфывапролджэячсмитьбю]") ||
			                                  	Regex.IsMatch(t, @"(ый|ой|ая|ий)$") ||
			                                  	File.ReadAllLines("../../notDeseases.txt").Any(e => e == t)
			                                  )
				).ToArray();
			return q.Distinct().OrderBy(s => s);
		}

		public IEnumerable<InvertedIndexUnit> GetIndex(IEnumerable<Tuple<long,string>> idTextList )
		{
			return DeseasesList.Select(item =>
			                       new InvertedIndexUnit(
			                       	item,
			                       	idTextList
			                       		.Where(t => t.Item2.SplitInWordsAndStripHTML().Any(s => s == item))
			                       		.Select(w => w.Item1)
			                       	))
									.Where(q => q.Ids.Count > 0 );
		}
		public IEnumerable<string> GetIndexFromQuestionList(QuestionList ql)
		{
			var des = new Deseases(Program.DefaultMyStemmer);
			return des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText)))
				.Select(q => q.Word + " " + String.Join(" ", q.Ids));
		}
	}

	[TestFixture]
	public class GetDeseases
	{
		[Test]
		public void Get()
		{
			var ql = Program.DefaultQuestionList;
			var des = new Deseases(new MyStemmer(new Vocabulary(Program.QuestionsFileName, Program.AnswersFileName)));
			Console.WriteLine("Начнем!");
			var deseasesIndex =
				des.GetIndex(ql.GetAllQuestions().Select(t => Tuple.Create(t.Id, t.WholeText)))
				.Select(q => q.Word + "\t" + String.Join(" ", q.Ids))
				.ToArray();
			File.WriteAllLines("deseasesIndex.txt", deseasesIndex);
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
