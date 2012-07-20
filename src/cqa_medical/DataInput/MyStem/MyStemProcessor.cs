using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace cqa_medical.DataInput.MyStem
{
	class MyStemProcessor
	{
		public readonly string FileName;

		/// <summary>
		///  It processes MyStem.exe output files.
		///  Format of the outpit file returns after following call string:
		///  <example>mystem.exe -in -e cp1251 input.txt output.txt</example>
		/// </summary>
		/// <param name="fileName">MyStem output file name.</param>
		public MyStemProcessor(string fileName)
		{
			FileName = fileName;
		}

		/// <param name="targetFileName">The name of file to save processed words</param>
		public void RemoveRedundancies(string targetFileName)
		{
			var reader = new StreamReader(FileName, Encoding.GetEncoding(1251));
			var processedLines = new HashSet<string>();
			while(!reader.EndOfStream)
			{
				var line = process(reader.ReadLine());
				if (line != "")
				{
					processedLines.Add(line);
				}
			}
			File.WriteAllText(targetFileName, String.Join("\n", processedLines));
		}

		private string process(string line)
		{
			var parts = line.Split(new[] {'{', '=', ',', '?', '}'}, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length > 2)
			{
				return parts[0] + "\t" + parts[1] + "\t" + parts[2];
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
			var parser = new MyStemProcessor("../../Files/qsv_stemmed.csv");
			var start = DateTime.Now;
			parser.RemoveRedundancies("../../Files/qst_stemmed.txt");
			Console.WriteLine("Question parsing: " + (DateTime.Now - start).TotalSeconds);
		}
	
		[Test]
		public void TestAnswersProcessor()
		{
			var parser = new MyStemProcessor("../../Files/ans_stemmed.txt");
			var start = DateTime.Now;
			parser.RemoveRedundancies("../../Files/ans_stemmed2.txt");
			Console.WriteLine("Question parsing: " + (DateTime.Now - start).TotalSeconds);
		}
	}


}
