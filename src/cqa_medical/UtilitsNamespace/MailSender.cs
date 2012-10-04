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
		public bool IsBodyHtml = false;

		public Dictionary<string, string> AdditionalHeaders; 


		private readonly NetworkCredential loginPasswordCredentials;
		private readonly SmtpClient smtp;

		public MailSender(string mailFrom, string password, string mailHost)
			:this(new NetworkCredential(mailFrom, password), mailHost)
		{
		}
		public MailSender(NetworkCredential loginAndPassword, string mailHost, int port = 25)
		{
			loginPasswordCredentials = loginAndPassword;
			smtp = new SmtpClient(mailHost, port) {Credentials = loginPasswordCredentials};
			AdditionalHeaders = new Dictionary<string, string>();
		}

		public void SendMail(string mailTo, string subject, string body )
		{
			var message = new MailMessage(loginPasswordCredentials.UserName, mailTo)
			                      	{
			                      		Subject = subject,
			                      		Body = body,
			                      		SubjectEncoding = MailEncoding,
			                      		BodyEncoding = MailEncoding,
			                      		Priority = Priority,
			                      		IsBodyHtml = IsBodyHtml,
			                      	};
			foreach (var pair in AdditionalHeaders)
			{
				message.Headers.Add(pair.Key, pair.Value);
			}
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
			AdditionalHeaders.Add("Precedence", "bulk");
			AdditionalHeaders.Add("X-Mailru-Msgtype", "Statistics");
			AdditionalHeaders.Add("X-Mailer", "statistics");

			var rand = new Random(1);
			foreach (var mail in mails)
			{
				SendMail(mail.MailTo, mail.Subject, mail.Body);
				Sleep(rand.Next(2000, 5000)); // I'm not a spammer
			}
			AdditionalHeaders.Remove("X-Mailer");
			AdditionalHeaders.Remove("Precedence");
			AdditionalHeaders.Remove("X-Mailru-Msgtype");
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
			var q = new MailSender("SenDer mail @mail.ru", "sender password", "smtp.mail.ru");
			var w = new[]
			        	{
							// можно сделать linq 
							// и вставлять имя получателя в тело письма через String.Format
			        		new MainMailInfo("", "Эта тема для тебя", "Будто я египтянин"), 
			        		new MainMailInfo("", "Эта тема для тебя", "I wanna WoofWoof"),
			        		new MainMailInfo("", "Эта тема для тебя", "Discovering buddhism"),
			        		new MainMailInfo("", "Эта тема для тебя", "<iframe src=\"https://docs.google.com/spreadsheet/embeddedform?formkey=dHlId3VneElKVUVEc1NfVFZrYUo1REE6MQ\" width=\"760\" height=\"2706\" frameborder=\"0\" marginheight=\"0\" marginwidth=\"0\">Загрузка...</iframe>")
			        	};
			q.SendALotOfMails(w);
		}
	}
}
