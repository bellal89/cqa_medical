using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;
using cqa_medical.Utilits;

namespace cqa_medical.SpellChecker
{
	class TypoDetecter
	{
		private readonly TrigramIndex trigramIndex;
	
		public TypoDetecter(TrigramIndex trigramIndex)
		{
			this.trigramIndex = trigramIndex;
		}

		public string Fix(string word)
		{
			return "";
		}

	}

	[TestFixture]
	public class TypoDetecterTest
	{
		[Test]
		public static void TestCreation()
		{			
			Console.WriteLine("Go!");
		}
	}
}
