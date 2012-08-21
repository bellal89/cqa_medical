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
		private static string DownloadUserPageString(string userName)
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
				"t=obLD1AAAAAAIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAABAAAAAAgAAABdAAUIxAcA; p=XmcAANX/1wAA; hses=1; VID=0LmJtv3mHIH0; b=zzwbAEDeoAEAAKMxMB/34MnfA2UeD6j8PbgI9iCT0QNYRg8ClThIVOJgF3uDNSUOkr5YjEOZ8Px0Qic/CZVXK3RCmZAoZcQftQi9IysMDBtR/mTILT8hC8MmBACAEFEtAmrjVmQ3LKhBtp/WIONPa5DupzVIU84K5PtpjVJTr0KZvVihvBEtYur1VQAA; mrcu=5688502CE27D6881E52FBBC9112E; Mpop=1345120771:7b0242604406726319050219081d00051c0c024f6a5d5e465e05030207021e0901711e4d5c4b451e455246435a471a060b105d57515e1c4a4c:test.testov.12@mail.ru:"
			);
			return webClient.DownloadString(url);
		}

		public static void DownloadUsersInto(IEnumerable<string> userList, string directoryToSave)
		{
			var i = 1;
			foreach (var userName in userList)
			{
				try
				{
					File.WriteAllText(directoryToSave + userName + ".html", DownloadUserPageString(userName));
				}
				catch(WebException e)
				{
					Console.WriteLine("Error 500: " + e.Response);
				}
				Console.WriteLine(i++);
				var r = new Random();
				Thread.Sleep(r.Next(300, 500));
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
				.Take(1)
				.Concat(questionList.GetAllAnswers().Select(a => a.AuthorEmail))
				.Distinct();

			MailUserPageDownloader.DownloadUsersInto(userList, Program.StatisticsDirectory + "userInfos/");
		}
	}
}
