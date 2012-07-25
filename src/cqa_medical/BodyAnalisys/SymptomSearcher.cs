using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers.MyStemmer;

namespace cqa_medical.BodyAnalisys
{
	class SymptomSearcher
	{
		private readonly Vocabulary vocabulary;
		private readonly QuestionList questionList;
		private readonly Dictionary<string, BodyPart> bodyParts;
		private Dictionary<string, List<string>> bodyPartToSymptoms;

		private const int Radius = 2;

		public SymptomSearcher(Vocabulary vocabulary, QuestionList questionList, BodyPart body)
		{
			this.vocabulary = vocabulary;
			this.questionList = questionList;
			bodyParts = body.ToDictionary();
		}

		public Dictionary<string, List<long>> GetSymptoms()
		{
			var partToSymptoms = new Dictionary<string, List<string>>();
			var symptomToQuestionList = new Dictionary<string, List<long>>();

			foreach (var question in questionList.GetAllQuestions())
			{
				var words = question.WholeText.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
				for (var i = 0; i < words.Length; i++)
				{
					if (!bodyParts.ContainsKey(words[i])) continue;
					var verbs = GetVerbs(words, i, Radius);
					if (verbs == null) continue;
					
					if (!partToSymptoms.ContainsKey(words[i]))
					{
						partToSymptoms.Add(words[i], new List<string>());
					}
					foreach (var verb in verbs)
					{
						partToSymptoms[words[i]].Add(verb);
						
						var symptom = words[i] + " " + verb;
						if (!symptomToQuestionList.ContainsKey(symptom))
							symptomToQuestionList.Add(symptom, new List<long>());
						symptomToQuestionList[symptom].Add(question.Id);
					}
				}
			}

			return symptomToQuestionList;
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

	}

	[TestFixture]
	public class SymptomSearchTest
	{
		[Test]
		public static void TestSearch()
		{
			var voc = new Vocabulary(Program.QuestionsFileName, Program.AnswersFileName);

			var parser = new Parser(Program.QuestionsFileName, Program.AnswersFileName);
			var questionList = new QuestionList();
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);
			questionList.StemIt(new MyStemmer(voc));

			var body = BodyPart.GetBodyPartsFromFile(Program.BodyPartsFileName);

			var searcher = new SymptomSearcher(voc, questionList, body);
			var symptoms = searcher.GetSymptoms();
			Console.WriteLine(symptoms);
		}
	}
}
