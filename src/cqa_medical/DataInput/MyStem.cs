using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace cqa_medical
{
	class MyStem
	{
		private Dictionary<long, String> text;
		public MyStem(IEnumerable<Tuple<long, string>> data)
		{
			foreach (var q in data)
			{
				text.Add(q.Item1, q.Item2.StripHTMLTags());
			}
		}

		public string UseStem(string s)
		{
			var process = Process.Start(@"..\..\Files\mystem.exe", "-lc AnswersTest.csv");
			if (process == null)
				throw new Exception("Cannot invoke MyStem");
			var reader = process.StandardOutput;
		}

		// сейчас надо взять текст из наших данных да еще и в одной строке и применить к нему MyStem
	}
}
