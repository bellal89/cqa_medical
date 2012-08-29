using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace
{
	public class MailUser
	{
		public string Email { get; private set; }
		public DateTime BirthDate { get; set; }
		public string Name { get; set; }
		public string Geo { get; set; }
		public Dictionary<string, string> Info = new Dictionary<string, string>();

		public MailUser(string email)
		{
			Email = email;
		}
	}

	class MailUserPageParser
	{
		private readonly string usersDirectory;
		private readonly HtmlDocument html = new HtmlDocument();

		public MailUserPageParser(string usersDirectory)
		{
			this.usersDirectory = usersDirectory;
		}

		public IEnumerable<MailUser> ParseUsers()
		{
			return Directory.GetFiles(usersDirectory).Select(ParseUser).Where(u => u != null).ToList();
		}

		private MailUser ParseUser(string fileName)
		{
			html.Load(fileName, Encoding.GetEncoding(1251));

			var fileNameParts = fileName.Split(new[] {'\\', '/'}, StringSplitOptions.RemoveEmptyEntries);
			var userEmail = fileNameParts[fileNameParts.Length - 1];
			userEmail = userEmail.Substring(0, userEmail.Length - 5);

			var content = html.GetElementbyId("centerColumn");
			if (content == null)
				return null;

			return content.ChildNodes.FindFirst("div").Id == "profile" ? GetProfile(userEmail, content) : GetDetailedUser(userEmail, content);
		}

		private static MailUser GetDetailedUser(string userEmail, HtmlNode content)
		{
			var bio = content.SelectSingleNode("dl[@class='cc_bio']");
			if (bio == null)
				throw new Exception("Detailed profile format is not correct!");

			var fields = bio.SelectNodes("dt|dd");
			if (fields == null || fields.Count == 0)
				return null;

			var user = new MailUser(userEmail);

			for (var i = 0; i < fields.Count - 1; i += 2)
			{
				var name = fields[i].InnerText.ToLower(CultureInfo.InvariantCulture).Trim(':', ' ', '"', '\t', '\n');
				var value = fields[i + 1].InnerText.ToLower(CultureInfo.InvariantCulture);
				switch (name)
				{
					case "откуда":
						user.Geo = value;
						break;
					case "день рождения":
						user.BirthDate = DateTime.Parse(value.Split(new[] {'(', '"'}, StringSplitOptions.RemoveEmptyEntries)[0].Trim());
						break;
					default:
						user.Info[name] = value;
						break;
				}
			}
			return user;
		}

		private static MailUser GetProfile(string userEmail, HtmlNode content)
		{
			var contentDiv = content.SelectSingleNode("div/div/div[@class='content']");
			if (contentDiv == null)
				throw new Exception("Profile format is not correct!");
			
			var user = new MailUser(userEmail);
			var name = contentDiv.SelectSingleNode("div/div[@class='header']/h1");
			if (name != null)
				user.Name = name.InnerText;
			
			var mainInfo = contentDiv.SelectSingleNode("div/div[@class='mainInfo']");
			if (mainInfo == null)
				return user;
				
			user.Info["additional"] = mainInfo.InnerText;
			
			var geo = mainInfo.SelectSingleNode("div/span");
			if (geo != null)
				user.Geo = geo.GetAttributeValue("title", "");

			return user;
		}
	}

	[TestFixture]
	public class MailParsingTest
	{
		[Test, Explicit]
		public static void MailUsersParsing()
		{
			var parser = new MailUserPageParser(Program.MailUsesDirectory);
			var mailUsers = parser.ParseUsers().ToArray();

			Console.WriteLine("Geo filled: " + ((double)mailUsers.Count(u => u.Geo != null)) / mailUsers.Count());
			Console.WriteLine(String.Join("\n", mailUsers.Where(u => u.Geo != null).GroupBy(u => u.Geo.Split(',').Last().Trim(), (key, keyUsers) => Tuple.Create(key, keyUsers.Count())).OrderByDescending(it => it.Item2).Select(it => it.Item1 + "\t" + it.Item2)));
		}
	}
}
