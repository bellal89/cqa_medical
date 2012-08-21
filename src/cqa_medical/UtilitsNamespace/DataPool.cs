using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;

namespace cqa_medical.UtilitsNamespace
{
	class DataPool
	{
		public static void ObjectToFile(string outputFile, object obj)
		{
			using (var stream = new BufferedStream(new FileStream(outputFile, FileMode.Create),5000))
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, obj);
			}
			
		}
		public static object FileToObject(string fileName)
		{
			byte[] binData = File.ReadAllBytes(fileName);
			var formatter = new BinaryFormatter();
			var ms = new MemoryStream(binData);
			return formatter.Deserialize(ms);
		}
	}

	[TestFixture]
	internal class DataPollTest
	{
		[Test]
		public static void Qwe()
		{
			var q = Program.DefaultQuestionList;
			Console.WriteLine(String.Join("\n",q.GetAllQuestions().Take(5).Select(Question.FormatStringWrite)));

			 DataPool.ObjectToFile("qwer.bin.txt",q);
		}
		[Test]
		public static void qwd()
		{
			var w = new Func<QuestionList>(
				() => (QuestionList)DataPool.FileToObject("qwer.bin.txt")).DetectTime("qwe");
			Console.WriteLine(String.Join("\n",w.GetAllQuestions().Take(5).Select(Question.FormatStringWrite)));
		}
	}
}
