﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace cqa_medical.DataInput.Stemmers.MyStemmer
{
	class MyStemProcessor
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
		public MyStemProcessor(params string[] qaFileNames)
		{
			QAFileNames = qaFileNames;
			foreach (var qaFileName in QAFileNames)
			{
				stemmedFileNames.Add(qaFileName + StemmedFileSuffix);
			}
		}

		public Dictionary<string, StemInfo> GetStemInfoVocabulary()
		{
			if (!stemmedFileNames.Any(name => File.Exists(name) && IsActual(name)))
				foreach (var name in QAFileNames)
					InvokeMystem(name, name + StemmedFileSuffix);

			var wordToStemInfo = new Dictionary<string, StemInfo>();
			foreach (var parts in stemmedFileNames
									.Select(File.ReadAllLines)
									.SelectMany(lines => lines, (lines, line) => line.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries))
									.Where(parts => parts.Length > 2 && !wordToStemInfo.ContainsKey(parts[0])))
			{
				wordToStemInfo.Add(parts[0], new StemInfo(parts[1], parts[2]));
			}
			return wordToStemInfo;
		}

		private bool IsActual(string fileName)
		{
			return QAFileNames.All(name => File.GetCreationTime(name) < File.GetCreationTime(fileName));
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

		/// <param name="inputFileName"> </param>
		/// <param name="outputFileName"> </param>
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
		[Test]
		public void TestQuestionsProcessor()
		{
			var start = DateTime.Now;
			var parser = new MyStemProcessor(Program.QuestionsFileName, Program.AnswersFileName);
			var voc = parser.GetStemInfoVocabulary();
			Console.WriteLine("Question parsing: " + (DateTime.Now - start).TotalSeconds);
			Console.WriteLine("Parsed: " + voc.Count);
		}
	}


}
