using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.DataInput
{
	internal class ObjectSerializer
	{
		public static byte[] SerializeToBytes<T>(T item)
		{
			var formatter = new BinaryFormatter();
			using (var stream = new MemoryStream())
			{
				formatter.Serialize(stream, item);
				stream.Seek(0, SeekOrigin.Begin);
				return stream.ToArray();
			}
		}

		public static object DeserializeFromBytes(byte[] bytes)
		{
			var formatter = new BinaryFormatter();
			using (var stream = new MemoryStream(bytes))
			{
				return formatter.Deserialize(stream);
			}
		}

		public static void SaveToFile<T>(T obj, string fileName)
		{
			var formatter = new BinaryFormatter();
			using (var stream = new BufferedStream(new FileStream(fileName, FileMode.Create), 4096))
			{
				formatter.Serialize(stream, obj);
			}
		}

		public static object GetFromFile(string fileName)
		{
			var formatter = new BinaryFormatter();
			using (var stream = new BufferedStream(new FileStream(fileName, FileMode.Open), 4096))
			{
				return formatter.Deserialize(stream);
			}
		}

		public static T GetFromFile<T>(string fileName)
		{
			return (T) GetFromFile(fileName);
		}
	}

	[TestFixture]
	internal class qwe
	{
		[Test]
		public void asd()
		{
			var fileName = "1.fg.kl";
			var q = new MailUser("trololo@ololo.ru");
			q.Info.Add("qwe", "rty");
			ObjectSerializer.SaveToFile(q, fileName);

			var w = ObjectSerializer.GetFromFile<MailUser>(fileName);
			Assert.AreEqual(q.Name,w.Name);
			Assert.AreEqual(q.BirthDate,w.BirthDate);
			Assert.AreEqual(q.Email,w.Email);
			Assert.AreEqual(q.Geo,w.Geo);
			Assert.AreEqual(q.Info,w.Info);
		}
	}

}
