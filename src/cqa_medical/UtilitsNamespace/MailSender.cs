using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace
{
	class MailSender
	{

		public Encoding MailEncoding = Encoding.UTF8;
		public MailPriority Priority = MailPriority.High;
		public bool IsBodyHtml = true;


		private readonly NetworkCredential loginPasswordCredentials;
		private readonly string mailHost;

		public MailSender(string mailFrom, string password, string mailHost)
			:this(new NetworkCredential(mailFrom, password), mailHost)
		{
		}

		public MailSender(NetworkCredential loginAndPassword, string mailHost)
		{
			this.mailHost = mailHost;
			loginPasswordCredentials = loginAndPassword;
		}

		public void SendMail(string mailTo, string subject, string body )
		{

			var smtp = new SmtpClient(mailHost, 25);

			var message = new MailMessage(loginPasswordCredentials.UserName, mailTo)
			                      	{
			                      		Subject = subject,
			                      		Body = body,
			                      		SubjectEncoding = MailEncoding,
			                      		BodyEncoding = MailEncoding,
			                      		Priority = Priority,
			                      		IsBodyHtml = IsBodyHtml
			                      	};
			
			smtp.Credentials = loginPasswordCredentials;
			smtp.Send(message); 
		}

		private static void Sleep(long millisecondsToWait)
		{
			var stopwatch = Stopwatch.StartNew();
			while (true)
			{
				if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
				{
					break;
				}
			}
		}

		public void SendALotOfMails(IEnumerable<MainMailInfo> mails)
		{
			var rand = new Random(1);
			foreach (var mail in mails)
			{
				SendMail(mail.MailTo, mail.Subject, mail.Body);
				Sleep(rand.Next(2000, 5000)); // I'm not a spammer
			}
		}
	}

	internal class MainMailInfo
	{
		public string MailTo;
		public string Subject;
		public string Body;

		public MainMailInfo(string mailTo, string subject, string body)
		{
			MailTo = mailTo;
			Subject = subject;
			Body = body;
		}
	}

	[TestFixture]
	internal class MailSenderTest
	{
		[Test]
		public void TestMailBroadcast()
		{
			// !!!!!!!!!!!!!!!!!!!!!!!
			// введи логин и пароль
			// нехочу заливать на github пароль и почту
			var q = new MailSender("test.testov.12@mail.ru", "Qwerty-123", "smtp.mail.ru");
			var w = new[]
			        	{
							// можно сделать linq 
							// и вставлять имя получателя в тело письма через String.Format
			        		new MainMailInfo("test.testov.12@mail.ru", "Эта тема для тебя", "Будто я египтянин"), 
			        		new MainMailInfo("bellal89@mail.ru", "Эта тема для тебя", "I wanna WoofWoof"),
			        		new MainMailInfo("valan89@gmail.com", "Эта тема для тебя", "Discovering buddhism"),
			        		new MainMailInfo("beloborodov@skbkontur.ru", "Эта тема для тебя", "<iframe src=\"https://docs.google.com/spreadsheet/embeddedform?formkey=dHlId3VneElKVUVEc1NfVFZrYUo1REE6MQ\" width=\"760\" height=\"2706\" frameborder=\"0\" marginheight=\"0\" marginwidth=\"0\">Загрузка...</iframe>")
			        	};
			q.SendALotOfMails(w);
		}
	}
}
