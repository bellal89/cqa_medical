using System;
using System.Collections;
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
		private string[] deseases;
		private readonly IStemmer stemmer;
		public Deseases(IStemmer stemmer, string filename)
		{
			deseases = File.ReadLines(filename).Select(s => s.Trim()).ToArray();

			this.stemmer = stemmer;
		}

		public Deseases(IStemmer stemmer, string[] deseases)
		{
			this.deseases = deseases;
			this.stemmer = stemmer;
		}

		// очень завязан на файл Deseases.txt
		public IOrderedEnumerable<string> GetDeseases()
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
			                                  	Regex.IsMatch(t, @"[^йцукенгшшщзхъфывапролджэячсмитьбю]") ||
			                                  	Regex.IsMatch(t, @"(ый|ой|ая|ий)$") ||
			                                  	File.ReadAllLines("../../notDeseases.txt").Any(e => e == t)
			                                  )
				).ToArray();
			return q.Distinct().OrderBy(s => s);
		}

		public static IOrderedEnumerable<string> GetDeseasesDefault()
		{
			var q = new Deseases(
				new MyStemmer(new Vocabulary(Program.QuestionsFileName, Program.AnswersFileName)),
				Program.DeseasesFileName
				);
			return q.GetDeseases();
		}
	}

	[TestFixture]
	public class GetDeseases
	{
		[Test]
		public void Get()
		{
			File.WriteAllLines("rightWords.txt",Deseases.GetDeseasesDefault());
		}
	}
}
