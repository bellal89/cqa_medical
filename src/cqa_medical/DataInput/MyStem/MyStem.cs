﻿using System;
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
		public static string Run(IEnumerable<string> strings)
		{
			var dataString = String.Join("\n", strings);
			var mystemAnswer = UseStem(dataString).Select(t => t.Word).ToArray();
			return String.Join(" ",mystemAnswer);
		}

		public static MyStemUnit[] UseStem(string s)
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

	//[TestFixture]
	//internal class MyStemTest
	//{
	//    [Test]
	//    public void TestMyStem()
	//    {
			// необходимо научиться кодировать строчки нормально
	//            //  var rawString = Encoding.GetEncoding("cpp866").GetString(reader.());
	//        var s = MyStem.UseStem(Encoding.GetEncoding("cpp866")..GetString("Что в моей внешности тебе не нравится?"));
	//        Assert.AreEqual("мой", s[2].Word);
	//    }

	//}
}
