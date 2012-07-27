using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;
using cqa_medical.Utilits;

namespace cqa_medical.BodyAnalisys
{
	class Symptoms
	{
		private readonly Vocabulary vocabulary;
		private readonly Dictionary<string, BodyPart> bodyParts;
		private Dictionary<string, List<string>> bodyPartToSymptoms;

		private const int Radius = 2;

		public Symptoms(Vocabulary vocabulary, BodyPart body)
		{
			this.vocabulary = vocabulary;
			bodyParts = body.ToDictionary();
		}

		public List<InvertedIndexUnit> GetSymptoms(IEnumerable<Tuple<long, string>> idAndTextList)
		{
			var symptomToQuestionList = new Dictionary<string, List<long>>();

			foreach (var pair in idAndTextList)
			{
				var words = pair.Item2.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
				for (var i = 0; i < words.Length; i++)
				{
					if (!bodyParts.ContainsKey(words[i])) continue;
					var verbs = GetVerbs(words, i, Radius);
					if (verbs == null) continue;
					
					foreach (var symptom in verbs.Select(verb => words[i] + "_" + verb))
					{
						if (!symptomToQuestionList.ContainsKey(symptom))
							symptomToQuestionList.Add(symptom, new List<long>());
						symptomToQuestionList[symptom].Add(pair.Item1);
					}
				}
			}

			return symptomToQuestionList.Select(item => new InvertedIndexUnit(item.Key, item.Value)).ToList();
		}

		private IEnumerable<string> GetVerbs(IList<string> words, int pos, int radius)
		{
			var verbs = new List<string>();
			var minPos = pos - radius >= 0 ? pos - radius : 0;
			var maxPos = pos + radius < words.Count ? pos + radius : words.Count - 1;
			for (var i = minPos; i <= maxPos; i++)
			{
				var partOfSpeech = vocabulary.GetPartOfSpeech(words[i]);
				if (partOfSpeech != null && partOfSpeech == "V")
				{
					verbs.Add(words[i]);
				}
			}
			return verbs;
		}
		public static IEnumerable<InvertedIndexUnit> GetDefault()
		{
			if (Utilits.Utilits.IsFileActual(Program.SymptomsIndexFileName, Program.QuestionsFileName))
			{
				var rawStrings = File.ReadAllLines(Program.SymptomsIndexFileName);
				return rawStrings.Select(s => new InvertedIndexUnit(s));
			}

			var voc = Program.DefaultVocabulary;
			var body = BodyPart.GetBodyPartsFromFile(Program.BodyPartsFileName);
			var searcher = new Symptoms(voc, body);

			var questionList = Program.DefaultQuestionList;

			var symptoms = searcher.GetSymptoms(questionList.GetAllQuestions().Select(item => Tuple.Create(item.Id, item.WholeText))).ToArray();
			File.WriteAllLines(Program.SymptomsIndexFileName, symptoms.Select(s => s.ToString()));

			return symptoms;
		}

	}

	[TestFixture]
	public class SymptomSearchTest
	{
		[Test]
		public static void TestSearch()
		{
			var voc = Program.DefaultVocabulary;
			var body = BodyPart.GetBodyPartsFromFile(Program.BodyPartsFileName);
			var searcher = new Symptoms(voc, body);

			var questionList = Program.TestDefaultQuestionList;

			var start = DateTime.Now;
			var symptoms = searcher.GetSymptoms(questionList.GetAllQuestions().Select(item => Tuple.Create(item.Id, item.WholeText)));
			Console.WriteLine("Symptoms found at {0} seconds.", (DateTime.Now - start).TotalSeconds);
			Console.WriteLine(String.Join("\n",symptoms.Select(s => s.ToString())));
		}

		[Test]
		public static void GetSymptoms()
		{
			var voc = new Vocabulary(Program.QuestionsFileName, Program.AnswersFileName);
			var body = BodyPart.GetBodyPartsFromFile(Program.BodyPartsFileName);
			var searcher = new Symptoms(voc, body);

			var questionList = Program.DefaultQuestionList;

			var start = DateTime.Now;
			var symptoms = searcher.GetSymptoms(questionList.GetAllQuestions().Select(item => Tuple.Create(item.Id, item.WholeText))).ToList();
			Console.WriteLine("Symptoms found at {0} seconds.", (DateTime.Now - start).TotalSeconds);
			File.WriteAllLines("SymptomIndex.txt", symptoms.Select(s => s.ToString()));
		}
	}
}
