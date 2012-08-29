using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.DataInput.Stemmers.MyStemmer
{
	class Vocabulary
	{
		private readonly Dictionary<string, StemInfo> wordToStemInfo;

		public Vocabulary(params string[] fileNames)
		{
			wordToStemInfo = new Dictionary<string, StemInfo>();
			wordToStemInfo =
				(new MyStemLoader(fileNames)).GetStemInfoVocabulary();
		}

		public IEnumerable<string> GetPartOfSpeech(string partOfSpeech, string text)
		{
			return text.SplitIntoWords()
				.Select(item => Tuple.Create(item, GetPartOfSpeech(item)))
				.Where(wordPoS => wordPoS.Item2 == partOfSpeech)
				.Select(wordPoS => wordPoS.Item1);
		}

		public string GetPartOfSpeech(string word)
		{
			return wordToStemInfo.ContainsKey(word) ? wordToStemInfo[word].PartOfSpeach : null;
		}

		public Dictionary<string, StemInfo> GetWordInfos()
		{
			return wordToStemInfo;
		}

		public StemInfo FindWordInfo(string word)
		{
			StemInfo wordInfo;
			return wordToStemInfo.TryGetValue(word, out wordInfo) ? wordInfo : null;
		}
		public IEnumerable<StemInfo> GetAllInfos()
		{
			return wordToStemInfo.Values;
		}
	}

	class MyStemLoader
	{
		private const string StemmedFileSuffix = ".stemmed.txt";
		public readonly string[] QAFileNames;
		private readonly List<string> stemmedFileNames = new List<string>();

		/// <summary>
		///  MyStem returns needed output file format after following call string:
		///  <example>mystem.exe -in -e cp1251 input.txt output.txt</example>
		///  If no mystem output files found it runs mystem to retrieve output files.
		/// </summary>
		/// <param name="qaFileNames">Community question answering data file name.</param>
		public MyStemLoader(params string[] qaFileNames)
		{
			QAFileNames = qaFileNames;
			foreach (var qaFileName in QAFileNames)
			{
				stemmedFileNames.Add(qaFileName + StemmedFileSuffix);
			}
		}

		public Dictionary<string, StemInfo> GetStemInfoVocabulary()
		{
			var wordToStemInfo = new Dictionary<string, StemInfo>();
			if (!stemmedFileNames.Any(name => DataActualityChecker.IsFileActual(name, QAFileNames)))
				foreach (var name in QAFileNames)
					InvokeMystem(name, name + StemmedFileSuffix);

			foreach (var lines in stemmedFileNames.Select(File.ReadAllLines))
				foreach (var line in lines)
				{
					var parts = line.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length > 2 && !wordToStemInfo.ContainsKey(parts[0]))
					{
						wordToStemInfo.Add(parts[0], new StemInfo(parts[1], parts[2]));
					}
				}
			return wordToStemInfo;
		}

		private void InvokeMystem(string inputFileName, string outputFileName)
		{
			using (var process = new Process
			                     	{
			                     		StartInfo =
			                     			{
			                     				FileName = "mystem.exe", 
												Arguments = "-in -e cp1251 " + inputFileName + " " + outputFileName,
												CreateNoWindow = true
			                     			}
			                     	})
			{
				process.Start();

				process.WaitForExit();
			}
			RemoveRedundancies(outputFileName, outputFileName);
		}

		public void RemoveRedundancies(string inputFileName, string outputFileName)
		{
			var processedLines = new HashSet<string>();
			using (var reader = new StreamReader(inputFileName, Encoding.GetEncoding(1251)))
			{
				while (!reader.EndOfStream)
				{
					var line = process(reader.ReadLine());
					if (line != "")
					{
						processedLines.Add(line);
					}
				}
			}
			File.WriteAllText(outputFileName, String.Join("\n", processedLines));
		}

		private string process(string line)
		{
			var parts = line.Split(new[] {'{', '=', ',', '?', '}'}, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length > 2)
			{
				return parts[0].ToLower() + "\t" + parts[1] + "\t" + parts[2];
			}
			return "";
		}
	}

	[TestFixture]
	class MyStemProcessorTest
	{
		[Test, Explicit]
		public void TestQuestionsProcessor()
		{
			var start = DateTime.Now;
			var loader = new MyStemLoader(Program.QuestionsFileName, Program.AnswersFileName);
			var voc = loader.GetStemInfoVocabulary();
			Console.WriteLine("Question parsing: " + (DateTime.Now - start).TotalSeconds);
			Console.WriteLine("Parsed: " + voc.Count);
		}
	}


}
