using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace
{
	class Cities
	{
		private readonly Dictionary<string, string> cities;

		private Cities(Dictionary<string, string> cities)
		{
			this.cities = cities;
		}

		public static Cities GetRussianCities()
		{
			var dict = new Dictionary<string, string>();
			var lines = File.ReadAllLines(Program.CitiesFileName)
				.Select(l => l.ToLower());
			foreach (var line in lines)
			{
				var openParenthesis = line.LastIndexOf('(');
				var closeParenthesis = line.LastIndexOf(')');
				var city = line.Substring(0, openParenthesis - 1);
				var domain = line.Substring(openParenthesis + 1, closeParenthesis - openParenthesis - 1);
				while (dict.ContainsKey(city))
					city += "!";
				dict.Add(city, domain);

			}
			return new Cities(dict);
		}

		// here is a problem with non-unique city name
		public  string GetDomain(string city)
		{
			return cities[city.ToLower()];
		}

		/// <summary>
		///  use for non-unique city name
		/// </summary>
		/// <param name="city"></param>
		/// <returns></returns>
		public  string[] GetDomains(string city)
		{
			city = city.ToLower();
			return cities.Keys.Where(c => c.StartsWith(city)).Select(q => cities[q]).ToArray();
		}
		public IEnumerable<string> GetAllCities()
		{
			return cities.Keys.Distinct();
		}
		public IEnumerable<string> GetAllDomains()
		{
			return cities.Values.Distinct();
		}


		public HashSet<string> GetAllCitiesInDomain(string domain)
		{
			domain = domain.ToLower();
			return new HashSet<string>(cities.Where(a => a.Value.StartsWith( domain)).Select(a =>
			                                                                       	{
			                                                                       		var q = a.Key;
																						return q.EndsWith("!") ? q.Substring(0, q.IndexOf('!')) : q;
			                                                                       	}));

		}
	}

	[TestFixture]
	internal class qwe
	{
		[Test]
		public void asd()
		{
			var q = Cities.GetRussianCities();
			var w = q.GetAllCitiesInDomain("московская область");

		}
	}
}
