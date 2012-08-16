using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace
{
	class GismeteoWeatherCollection
	{
		private readonly List<DayWeather> weather = new List<DayWeather>();

		public GismeteoWeatherCollection(int cityCode, DateTime from, DateTime to)
		{
			for (var d = from; d <= to; d = d.AddMonths(1))
			{
				weather.AddRange((new GismeteoHTMLParser(cityCode, d.Year, d.Month)).GetMonthWeatherList());
			}

			weather = weather.Where(item => from <= item.Date && item.Date <= to).ToList();
		}

		public List<DayWeather> GetWeather()
		{
			return weather;
		}

		public void SaveTo(string fileName)
		{
			File.WriteAllText(fileName, String.Join("\n",
			            weather.Select(
			            	d =>
			            	d.Date + "\t" + ((d.DayTemperature + d.EveningTemperature)*1.0/2) + "\t" +
			            	((d.DayPressure + d.EveningPressure)*1.0/2))));
		}

		public void SaveDayInfoTo(string fileName)
		{
			File.WriteAllText(fileName, String.Join("\n",
						weather.Select(d => d.Date + "\t" + d.DayTemperature + "\t" + d.DayPressure)));
		}

		public void SaveEveningInfoTo(string fileName)
		{
			File.WriteAllText(fileName, String.Join("\n",
						weather.Select(d => d.Date + "\t" + d.EveningTemperature + "\t" + d.EveningPressure)));
		}
	}

	class GismeteoHTMLParser
	{
		private readonly int cityCode;
		private readonly int year;
		private readonly int month;
		private readonly string url;
		private readonly IEnumerable<DayWeather> dayWeathers; 

		public GismeteoHTMLParser(int cityCode, int year, int month)
		{
			this.cityCode = cityCode;
			this.year = year;
			this.month = month;

			url = string.Format("http://www.gismeteo.ru/diary/{0}/{1}/{2}/", cityCode, year, month);
			var html = new HtmlDocument();
			html.LoadHtml(DownloadHTML(url));
			dayWeathers = GetDayWeathersFromPage(html);
		}

		private static string DownloadHTML(string url)
		{
			var webClient = new WebClient();
			return webClient.DownloadString(url);
		}

		private IEnumerable<DayWeather> GetDayWeathersFromPage(HtmlDocument html)
		{
			var table = html.GetElementbyId("data_block").ChildNodes.FirstOrDefault(node => node.Name == "table");
			if (table == null)
				throw new Exception("Invalid page!");

			foreach (var node in table.ChildNodes.FindFirst("tbody").ChildNodes)
			{
				if (node.Name == "tr")
				{
					if (node.ChildNodes.Count >= 8)
					{
						var day = ParseIntOrZero(node, 0);
						try
						{
							new DateTime(year, month, day);
						}
						catch
						{
							continue;
						}
						yield return new DayWeather
						             	{
						             		CityCode = cityCode,
						             		Date = new DateTime(year, month, day),
						             		DayTemperature = ParseIntOrZero(node, 1),
						             		DayPressure = ParseIntOrZero(node, 2),
						             		EveningTemperature = ParseIntOrZero(node, 6),
						             		EveningPressure = ParseIntOrZero(node, 7)
						             	};
					}
				}
			}
		}

		private static int ParseIntOrZero(HtmlNode tr, int index)
		{
			int result;
			return Int32.TryParse(tr.ChildNodes.Where(node => node.Name == "td").ElementAt(index).InnerText, out result) ? result : 0;
		}

		public Dictionary<DateTime, DayWeather> GetMonthWeatherDict()
		{
			return dayWeathers.ToDictionary(w => w.Date, w => w);
		}

		public List<DayWeather> GetMonthWeatherList()
		{
			return dayWeathers.ToList();
		}

		public IEnumerable<DayWeather> GetMonthWeather()
		{
			return dayWeathers;
		}
	}
	
	public class DayWeather
	{
		public int CityCode { get; set; }
		public DateTime Date { get; set; }
		public int DayTemperature { get; set; }
		public int DayPressure { get; set; }
		public int EveningTemperature { get; set; }
		public int EveningPressure { get; set; }
	}

	[TestFixture]
	public class MonthWeatherHTMLParserTest
	{
		[Test, Explicit]
		public static void RunWeatherRetrieval()
		{
			// 4368 = Moscow code
			var collection = new GismeteoWeatherCollection(4368, new DateTime(2011, 04, 1), new DateTime(2012, 03, 31));
			collection.SaveTo("../../Files/MoscowWeather2011-2012.txt");
			collection.SaveDayInfoTo("../../Files/MoscowDayWeather2011-2012.txt");
			collection.SaveEveningInfoTo("../../Files/MoscowEveningWeather2011-2012.txt");
		}

		[Test]
		public static void TestWeatherParsing()
		{
			var parser = new GismeteoHTMLParser(4368, 2011, 4);
			Assert.AreEqual(30, parser.GetMonthWeatherList().Count);
		}

		[Test]
		public static void TestYearWeatherRetrieval()
		{
			var collection = new GismeteoWeatherCollection(4368, new DateTime(2011, 03, 31), new DateTime(2012, 03, 31));
			Assert.AreEqual(365, collection.GetWeather().Count);
		}
	}
}