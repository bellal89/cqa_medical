using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

		public Dictionary<DateTime, double> GetWeekEveningTemperature()
		{
			var dict = new Dictionary<DateTime, int>();
			foreach (var day in weather.Where(day => !dict.ContainsKey(day.Date)))
			{
				dict[day.Date] = day.EveningTemperature;
			}
				
			return GetTemperatureByWeeks(dict);
		}

		private static Dictionary<DateTime, double> GetTemperatureByWeeks(IDictionary<DateTime, int> weatherDictionary)
		{
			return weatherDictionary.SumUpToWeeks().ToDictionary(kv => kv.Key, kv => ((double) kv.Value)/7);
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


		public Dictionary<DateTime, int> GetWeekCloudinessDistribution()
		{
			return weather.Select(w => Tuple.Create(w.Date.GetWeek(), w.DayCloudiness))
							  .GroupBy(w => w.Item1, (key, items) => Tuple.Create(key, items.Sum(it => (int)it.Item2)))
							  .ToDictionary(it => it.Item1, it => it.Item2);
		}

		public Dictionary<DateTime, int> GetWeekRainDistribution()
		{
			return weather.GroupBy(w => w.Date.GetWeek(), (key, ws) => Tuple.Create(key, ws.Sum(w =>
			                                                                             	{
			                                                                             		var n = 0;
																								if (w.DayFallout == Fallout.Rain) n++;
																								if (w.EveningFallout == Fallout.Rain) n++;
			                                                                             		return n;
			                                                                             	})))
																							.ToDictionary(it => it.Item1, it => it.Item2);
		}

		public Dictionary<DateTime, double> GetWeekPressureDistribution()
		{
			var pressure = weather
				.GroupBy(w => w.Date.GetWeek(), (key, ws) => Tuple.Create(key, (double) ws.Sum(w => w.DayPressure)/ws.Count())).ToList();
			var min = pressure.Min(it => it.Item2);
			return pressure.ToDictionary(it => it.Item1, it => it.Item2 - min);
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
											DayCloudiness = GetCloudiness(node, 3),
											DayFallout = GetFallout(node, 4),

						             		EveningTemperature = ParseIntOrZero(node, 6),
						             		EveningPressure = ParseIntOrZero(node, 7),
											EveningCloudiness = GetCloudiness(node, 8),
											EveningFallout = GetFallout(node, 9)
						             	};
					}
				}
			}
		}

		private static HtmlAttribute GetImgSrc(HtmlNode tr, int index)
		{
			var img =
				tr.ChildNodes.Where(node => node.Name == "td").ElementAt(index).ChildNodes.FirstOrDefault(node => node.Name == "img");
			return img == null ? null : img.Attributes.AttributesWithName("src").FirstOrDefault();
		}

		private static Fallout GetFallout(HtmlNode tr, int index)
		{
			var src = GetImgSrc(tr, index);
			if (src == null) return Fallout.NoFallout;
			if (src.Value.EndsWith("snow.png")) return Fallout.Snow;
			return src.Value.EndsWith("rain.png") ? Fallout.Rain : Fallout.NoFallout;
		}

		private static Cloudiness GetCloudiness(HtmlNode tr, int index)
		{
			var src = GetImgSrc(tr, index);
			if (src == null) return Cloudiness.SemiCloudy;
			if (src.Value.EndsWith("dull.png")) return Cloudiness.Cloudy;
			if (src.Value.EndsWith("sunc.png")) return Cloudiness.QuarterCloudy;
			return src.Value.EndsWith("sun.png") ? Cloudiness.NonCloudy : Cloudiness.SemiCloudy;
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
	
	public enum Cloudiness
	{
		NonCloudy = 0,
		QuarterCloudy = 1,
		SemiCloudy = 2,
		Cloudy = 4
	}

	public enum Fallout
	{
		NoFallout,
		Rain,
		Snow
	}

	public class DayWeather
	{
		public int CityCode { get; set; }
		public DateTime Date { get; set; }
		
		public int DayTemperature { get; set; }
		public int DayPressure { get; set; }
		public Cloudiness DayCloudiness { get; set; }
		public Fallout DayFallout { get; set; }

		public int EveningTemperature { get; set; }
		public int EveningPressure { get; set; }
		public Cloudiness EveningCloudiness{ get; set; }
		public Fallout EveningFallout { get; set; }
	}

	[TestFixture]
	public class MonthWeatherHTMLParserTest
	{
		// 4368 = Moscow code
		private static readonly GismeteoWeatherCollection WeatherCollection = new GismeteoWeatherCollection(4368, new DateTime(2011, 03, 27), new DateTime(2012, 03, 31));

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

		[Test, Explicit]
		public static void RunWeatherRetrieval()
		{
			// 4368 = Moscow code
			var collection = new GismeteoWeatherCollection(4368, new DateTime(2011, 04, 1), new DateTime(2012, 03, 31));
			collection.SaveTo("../../Files/MoscowWeather2011-2012.txt");
			collection.SaveDayInfoTo("../../Files/MoscowDayWeather2011-2012.txt");
			collection.SaveEveningInfoTo("../../Files/MoscowEveningWeather2011-2012.txt");
		}

		[Test, Explicit]
		public static void GetWeekEveningTemperature()
		{
			const string fileToSave = "MoscowWeekEveningTemperature.txt";
			File.WriteAllLines(fileToSave, WeatherCollection.GetWeekEveningTemperature().NormalizeByMax().Select(w => w.Key + "\t" + w.Value));
		}

		[Test, Explicit]
		public static void GetWeekDayPressure()
		{
			const string fileToSave = "MoscowWeekDayPressure.txt";
			File.WriteAllLines(fileToSave, WeatherCollection.GetWeekPressureDistribution().NormalizeByMax().Select(w => w.Key + "\t" + w.Value));
		}

		[Test, Explicit]
		public static void GetWeekCloudiness()
		{
			const string fileToSave = "MoscowWeekCloudiness.txt";
			File.WriteAllLines(fileToSave, WeatherCollection.GetWeekCloudinessDistribution().Select(w => w.Key + "\t" + w.Value));
		}

		[Test, Explicit]
		public static void GetWeekFalloutDistribution()
		{
			const string fileToSave = "MoscowWeekFallouts.txt";
			File.WriteAllLines(fileToSave, WeatherCollection.GetWeekRainDistribution().NormalizeByMax().Select(w => w.Key + "\t" + w.Value));
		}
	}
}