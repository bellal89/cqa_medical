using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace
{
	static class MailUserPageDownloader
	{
		private static byte[] DownloadUserPageBytes(string userName)
		{
			var parts = userName.Split(new[]{'@'}, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2) Console.WriteLine("User name is not correct: " + userName);
			var login = parts[0];
			var domainParts = parts[1].Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
			if (domainParts.Length != 2) Console.WriteLine("User mail domain is not correct: " + parts[1]);
			var domain = domainParts[0];

			var url = String.Format("http://my.mail.ru/{0}/{1}/info", domain, login);
			var webClient = new WebClient();
//			webClient.Encoding = Encoding.GetEncoding("unicode")
			webClient.Headers.Add(
				HttpRequestHeader.Cookie,
				"p=XmcAANX/1wAA; searchuid=1684819721341830973; __utma=56108983.898779550.1345184118.1345184118.1345184118.1; __utmc=56108983; __utmz=56108983.1345184118.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); i=AQCsES5QAQATAAgHAh8AASEAAQ==; s=dpr=1|rt=0; c=+hEuUAEAAJtZAAAiAAAAAwAI; t=obLD1AAAAAAIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAABAAAAAAgAAABdAAUIxAcA; b=1jwDAEAiygQACmwyBAAA0WIQRChPJIQW4o5RFiqKKA0VRZypfjJ8sjYJ; Mpop=1345724146:71524141657f767f19050219081d00051c0c024f6a5d5e465e05030207021e0901711e4d5c4b451e455246435a471a060b105d57515e1c4a4c:test.testov.12@mail.ru:;"
			);
			return webClient.DownloadData(url);
		}

		public static void DownloadUsersInto(IEnumerable<string> userList, string directoryToSave)
		{
			var i = 1;
			foreach (var userName in userList)
			{
				try
				{
					File.WriteAllBytes(directoryToSave + userName + ".html", DownloadUserPageBytes(userName));
				}
				catch(WebException e)
				{
					Console.WriteLine("Error 500: " + e.Response);
				}
				Console.WriteLine(i++);
				var r = new Random();
				Thread.Sleep(r.Next(2000, 4000));
			}
		}
	}

	[TestFixture]
	public class MailUsersDownloaderTest
	{
		[Test]
		public static void DownloadMailUsers()
		{
			var questionList = Program.DefaultQuestionList;
			var userList = questionList.GetAllQuestions()
				.Select(q => q.AuthorEmail)
				//.Concat(questionList.GetAllAnswers().Select(a => a.AuthorEmail))
				.Distinct().ToList();
			Console.WriteLine(userList.Count());
			MailUserPageDownloader.DownloadUsersInto(userList, Program.StatisticsDirectory + "userInfos2/");
		}
	}
}
