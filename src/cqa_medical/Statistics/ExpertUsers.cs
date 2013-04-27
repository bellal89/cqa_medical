using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.UtilitsNamespace;

namespace cqa_medical.Statistics
{
	class ExpertUsers
	{
		private readonly Statistics statistics;
		private readonly QuestionList ql;

		public ExpertUsers(QuestionList ql)
		{
			statistics = new Statistics(ql);
			this.ql = ql;
		}

		public IEnumerable<UserInfo> UsersFiltered(int minAnswersAmount, double minAuthorRating)
		{
			var answersAmount = statistics.UserActivityInAnswersDistibution();
			var users = ql.GetAllAnswers();
			return
				users
					.Where(u => answersAmount[u.AuthorEmail] > minAnswersAmount)
					.Where(u => u.AuthorEfficiency > minAuthorRating)
					.Select(u => new UserInfo(u.AuthorEmail, u.AuthorEfficiency, answersAmount[u.AuthorEmail]))
					.Distinct();
		}

		public static IEnumerable<UserInfo> GetDefault()
		{
			var ql = Program.DefaultQuestionList.NewQuestionListFilteredByCategories("illness");
			var experts = new ExpertUsers(ql).UsersFiltered(10, 0.15);
			var ans = experts.OrderByDescending(u => u.AnswersAmount * u.Rating).Take(1000);
			return ans;
		}
	}

	internal class UserInfo
	{
		public readonly string Email;
		public readonly double Rating;
		public readonly int AnswersAmount;

		public UserInfo(string email, double rating, int answersAmount)
		{
			Email = email;
			Rating = rating;
			AnswersAmount = answersAmount;
		}

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}", Email, Rating, AnswersAmount);
		}

		public bool Equals(UserInfo other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Email, Email) && other.Rating.Equals(Rating) && other.AnswersAmount == AnswersAmount;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (UserInfo)) return false;
			return Equals((UserInfo) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (Email != null ? Email.GetHashCode() : 0);
				result = (result*397) ^ Rating.GetHashCode();
				result = (result*397) ^ AnswersAmount;
				return result;
			}
		}
	}

	[TestFixture]
	internal class ExpertUserFind
	{
		[Test]
		public void Find()
		{
			var experts = ExpertUsers.GetDefault().Reverse().Take(200).ToList();
			Console.WriteLine(experts.Count);
			Console.WriteLine(String.Join("\n", experts.Select(u => u.ToString())));

			Console.WriteLine("============");

			var ans = ExpertUsers.GetDefault().ToList();
			Console.WriteLine(ans.Count);
			Console.WriteLine(String.Join("\n", ans.Select(u => u.ToString())));
			//File.WriteAllLines("userInMessages.txt", ans.Select(u => u.ToString()));
		}

		[Test]
		public void SendAnkets()
		{
			var formUrls = new[]
			{
				"https://docs.google.com/spreadsheet/viewform?formkey=dDRubE9RcWdTMkFucTBWcE01MDJoVHc6MA#gid=0",
				"https://docs.google.com/spreadsheet/viewform?formkey=dDR4YjBvMlgwNzdKc3VZaHVta05jLVE6MA#gid=0",
				"https://docs.google.com/spreadsheet/viewform?formkey=dEZRdUszMWlrdnNCNE1yWWpwcEM3UWc6MA#gid=0",
				"https://docs.google.com/spreadsheet/viewform?formkey=dHFDVHJHbUh5eXVxaDg5Qld4YXVOS0E6MA#gid=0",
				"https://docs.google.com/spreadsheet/viewform?formkey=dEpNbGltSGtZTW54VmtSTVZ4Z0Z6aUE6MA#gid=0",
				"https://docs.google.com/spreadsheet/viewform?formkey=dFZmRjFQa1phT2hzWlNfQTBUN0JDanc6MA#gid=0",
				"https://docs.google.com/spreadsheet/viewform?formkey=dGZCRUJGTmNWbTJSeG9aSWFiS3VGT2c6MA#gid=0",
				"https://docs.google.com/spreadsheet/viewform?formkey=dDc0bGRpdTAyWi1ZTzlBeTllR2stZXc6MA#gid=0"
			};

			var mailBodyVariants = new []{
@"Здравствуйте!

Нас зовут Павел Браславский и Александр Белобородов, наша научная группа проводит исследование Интернет-сервисов вопросов и ответов. В настоящее время нами проводится опрос активных пользователей сервиса Ответы@Mail.Ru в категории 'Красота и Здоровье'. Мы не связаны с разработчиками сервиса и с компанией Mail.Ru.

Если у вас есть немного свободного времени, ответьте на вопросы нашей анкеты. Она основана на технологии Google Docs и располагается в сети Интернет по адресу:
{0}
Опрос займет не более 7 минут, можно ответить лишь на часть вопросов.

Основываясь на мнении пользователей, мы проанализируем, как пользователи делятся своими знаниями. С удовольствием вышлем вам результаты опроса, если вас это заинтересует.

С наилучшими пожеланиями,
Павел Браславский, Александр Белобородов
",
@"Здравствуйте!

Наша научная группа в Уральском Федеральном университете проводит исследование сервисов вопросов и ответов в Интернете. На данный момент мы анкетирование активных пользователей сервиса Ответы@Mail.Ru в категории 'Красота и Здоровье'. Мы не связаны с разработчиками сервиса и с компанией Mail.Ru.

Вы можете нам помочь, ответив на вопросы анкеты. Она располагается в сети Интернет по адресу:
{0}
Опрос не займет много времени, вы можете ответить только на часть вопросов.

Ваше мнение поможет нам проанализировать и понять, как пользователи делятся своим жизненным опытом в вопросно-ответных сервисах. Если вас заинтересуют результаты опроса, с удовольствием вышлем их вам.

С наилучшими пожеланиями,
Александр Белобородов,
Уральский федеральный университет
",
@"Здравствуйте!

Меня зовут Александр, наша научная группа занимается исследованиями социальных вопросно-ответных сервисов. На данный момент нас интересует мнение активных пользователей сервиса Ответы@Mail.Ru в категории 'Красота и Здоровье'. Мы не связаны с разработчиками сервиса и с компанией Mail.Ru.

Ответьте на вопросы нашей анкеты, если у вас есть немного свободного времени. Анкета сделана с помощью сервиса Google Docs и располагается на сервере Google по адресу:
{0}
Вы можете ответить только на часть вопросов.

Мнение пользователей сервиса поможет понять, как пользователи делятся своими жизненным опытом и знаниями друг с другом. Если вас заинтересуют результаты опроса, мы с удовольствием их вышлем.

С наилучшими пожеланиями,
Александр Белобородов
",
 @"Здравствуйте!

Мое имя Павел Браславский, наша научная группа занимается анализом вопросно-ответных сервисов в сети Интернет. В настоящее время мы исследуем сервис Ответы@Mail.Ru, категорию 'Красота и Здоровье'. Мы не связаны с компанией Mail.Ru и разработчиками сервиса Ответы@Mail.Ru.

Если вас не затруднит, ответьте на вопросы нашей анкеты. Она располагается в сети Интернет по адресу:
{0}
Опрос займет не более 5-7 минут.

Ваше мнение поможет нашей научной группе проанализировать, как пользователи делятся своим жизненным опытом внутри вопросно-ответного сервиса. С удовольствием вышлем результаты опроса, если вас это заинтересует.

С наилучшими пожеланиями,
Павел Браславский
",
 @"Добрый день!

Наша научная группа исследует сервисы вопросов и ответов в сети Интернет. Мы проводим анкетирование пользователей сервиса Ответы@Mail.Ru в категории 'Красота и Здоровье'. Мы не связаны с разработчиками сервиса.

Ответьте на вопросы нашей анкеты, если вас не затруднит. Она создана в сервисе Google Docs и располагается по адресу:
{0}
Опрос займет 5-7 минут, можно ответить только на часть вопросов.

Основываясь на мнении пользователей, мы попытаемся понять, как пользователи делятся своими знаниями внутри сервисов вопросов и ответов. С удовольствием вышлем результаты опроса, если вас это заинтересует.

С наилучшими пожеланиями,
научная группа Уральского Федерального университета
",
 @"Добрый день!

Научная группа Уральского Федерального университета проводит исследование Интернет-сервисов вопросов и ответов. В данный момент мы исследуем сервис Ответы@Mail.Ru и опрашиваем активных пользователей в категории 'Красота и Здоровье'. Мы не связаны с разработчиками сервиса Ответы@Mail.Ru.

Если вас не затруднит, уделите нашей анкете 5-7 минут - это не займет больше времени. Она основана на технологии Google Docs и располагается в сети Интернет по адресу:
{0}
По желанию, вы можете ответить лишь на часть вопросов.

Ваше мнение поможет проанализировать процесс обмена знаниями и жизненным опытом среди пользователей сервиса. Если вас это заинтересуют результаты опроса, мы с удовольствием их вышлем.

С наилучшими пожеланиями,
Павел Браславский, Александр Белобородов,
научная группа Уральского Федерального университета
",
 @"Добрый день!

Меня зовут Павел, наша научная группа проводит исследование социальных вопросно-ответных Интернет-сервисов. Мы проводим опрос среди активных пользователей сервиса Ответы@Mail.Ru в категории 'Красота и Здоровье'.

Если у вас есть 5-7 минут, пожалуйста, ответьте на вопросы нашей анкеты. Она располагается в сети Интернет на сервере Google Docs и, поэтому, безопасна для вашего компьютера:
{0}

Ваше мнение поможет нам понять процессы обмена жизненным опытом среди пользователей сервиса Ответы@Mail.Ru. С удовольствием вышлем результаты опроса, если вас это заинтересует.

Всего доброго,
Павел Браславский
",
@"Добрый день!

Наша научная группа из Уральского Федерального университета занимается вопросами обмена знаниями внутри вопросно-ответных сервисов в сети Интернет. Сейчас мы исследуем сервис Ответы@Mail.Ru, категорию 'Красота и Здоровье'. Мы не связаны с разработчиками сервиса и с компанией Mail.Ru.

Мы проводим опрос активных пользователей. Если вас не затруднит, ответьте, пожалуйста, на вопросы нашей анкеты. Она располагается в сети Интернет (вам не придется ничего скачивать, поэтому это безопасно для вашего компьютера) по адресу:
{0}
Анкетирование займет не более 7 минут, можно ответить лишь на часть вопросов.

Основываясь на мнении пользователей, мы пытаемся понять, как пользователи делятся своими знаниями и опытом друг с другом. Если вас заинтересуют результаты анкетирования, с удовольствием поделимся.

С наилучшими пожеланиями,
Научная группа Уральского Федерального университета
"
};

			var mailSenders = new List<MailSender>
			                  	{
			                  		new MailSender("questions_research@mail.ru", "qwe-123", "smtp.mail.ru"),
									new MailSender("answers_research@mail.ru", "qwe-123", "smtp.mail.ru"),
									new MailSender("qa_research@mail.ru", "qwe-123", "smtp.mail.ru"),
									new MailSender("research_qa@mail.ru", "qwe-123", "smtp.mail.ru"),
									new MailSender("quest_research@mail.ru", "qwe-123", "smtp.mail.ru"),
									new MailSender("research_quest@mail.ru", "qwe-123", "smtp.mail.ru"),
									new MailSender("research_answers@mail.ru", "qwe-123", "smtp.mail.ru"),
									new MailSender("research_questions@mail.ru", "qwe-123", "smtp.mail.ru")
			                  	};
			const string mailSubject = "Исследование вопросно-ответного сервиса Ответы@Mail.Ru";

			var rand = new Random(1);
			var experts = ExpertUsers.GetDefault().Take(800).Reverse().Take(696).ToList();
			
			for (var j = 0; j < 696; j += 8)
			{
				for (var i = 0; i < 8; i++)
				{
					var u = experts[j + i];
					mailSenders[i].SendALotOfMails(new List<MainMailInfo> { new MainMailInfo(u.Email, mailSubject, String.Format(mailBodyVariants[i], formUrls[i])) });
				}
				System.Threading.Thread.Sleep(rand.Next(70000, 80000));
			}
		}
	}

}
