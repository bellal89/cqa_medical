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
		// сейчас надо взять текст из наших данных да еще и в одной строке и применить к нему MyStem
	}
}
