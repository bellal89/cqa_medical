using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.MyStem;


namespace cqa_medical
{
	class MyStem
	{
		public static IEnumerable<string> Run(IEnumerable<string> strings)
		{

			foreach (var s in strings)
			{
				// надо сбуферизовать штук 1000
				var mystemAnswer = UseStem(s).Select(t => t.Word);

				yield return String.Join(" ", mystemAnswer);
			}

			//var dataString = String.Join("\n", strings);
			//var mystemAnswer = UseStem(dataString).Select(t => t.Word).ToArray();
			//return String.Join(" ",mystemAnswer);
		}

		public static  MyStemUnit[] UseStem(string s)
		{
			using( var process = new Process())
			{
				process.StartInfo.Arguments = "-ln -e cp866";
				process.StartInfo.FileName = @"..\..\DataInput\MyStem\mystem.exe";

				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("cp866");
				process.Start();

				process.StandardInput.WriteLine(s);
				process.StandardInput.Close();
				var reader = process.StandardOutput;

				var rawString = reader.ReadToEnd();
				return rawString.Split('\r', '\n').Where(t => t != "").Select(t => new MyStemUnit(t)).ToArray();
			}
		}
	}
	
	//var rawData = questionList.GetAllQuestions().Select(r => r.Title + r.Text);
	//        String[] noHTMLData = rawData.Select(t => t.StripHTMLTags()).ToArray();
	//        int currentLineNumber = 0;
	//        using (var dataFile = new StreamWriter("DictionaryQuestions.txt", false, Encoding.GetEncoding(866)))
	//        {
	//            foreach (string q in MyStem.Run(rawData.Select(t => t.StripHTMLTags())).ToArray())
	//            {
	//                dataFile.WriteLine(q);
	//                ++currentLineNumber;
	//                if (currentLineNumber % 500 == 0)
	//                {
	//                    Console.WriteLine(currentLineNumber);
	//                    dataFile.Flush();
	//                }
	//            }
	//        }
}
