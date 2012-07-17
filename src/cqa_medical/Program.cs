using System;
using System.IO;
using System.Linq;
using cqa_medical.DataInput;
using cqa_medical.Statistics;
using cqa_medical.BodyAnalisys;

namespace cqa_medical
{
    class Program
    {
		private static void ParseAndStem()
		{
			const string questionsFileName = "../../Files/qst_25.csv";
			const string answersFileName = "../../Files/ans_25.csv";
			const string statisticsDirectory = "../../StatOutput/";
			var questionList = new QuestionList();
			DateTime start;

			start = DateTime.Now;
			var parser = new Parser(questionsFileName, answersFileName);
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);


			Console.WriteLine(String.Format("Parsing Completed in {0}", (DateTime.Now - start).TotalSeconds));
			questionList.StemIt();
		}


        static void Main(string[] args)
        {
        	var q = BodyPart.GetBodyPartsFromFile(@"..\..\Files\BodyParts.txt");
			
			var text = String.Join(" ", questionList.GetAllQuestions().Select(q => q.Title + " " + q.Text));
        	var freqs = new TextFrequencies(text);
        	var oneWordDictionary = freqs.GetOneWordDictionary().OrderByDescending(item => item.Value);
        	File.WriteAllText(statisticsDirectory + "QuestionsOneWordFreqs.txt", String.Join("\n", oneWordDictionary.Select(item => item.Key + "\t" + item.Value)));

			text = String.Join(" ", questionList.GetAllAnswers().Select(q => q.Text));
			freqs = new TextFrequencies(text);
			oneWordDictionary = freqs.GetOneWordDictionary().OrderByDescending(item => item.Value);
			File.WriteAllText(statisticsDirectory + "AnswersOneWordFreqs.txt", String.Join("\n", oneWordDictionary.Select(item => item.Key + "\t" + item.Value)));
        }

    	
    }
}
