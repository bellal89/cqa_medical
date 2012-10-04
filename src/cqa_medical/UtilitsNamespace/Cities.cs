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
		private readonly Dictionary<string, string> cities = new Dictionary<string, string>();
		private readonly Dictionary<string, string> domains = new Dictionary<string, string>(); 

		public Cities(string filename)
		{
			var lines = File.ReadAllLines(filename).Select(l => l.ToLower());
			foreach (var line in lines)
			{
				var parts = line.Split(new[] {'(', ')'}, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2) continue;

				var city = parts[0].Trim();
				var domain = parts[1].Trim();
				var domainName = domain.Replace("республика", "").Replace("край", "").Replace("область", "").Trim();

				if (!domains.ContainsKey(domainName))
					domains[domainName] = domain;

				while (cities.ContainsKey(city))
					city += "!";
				cities.Add(city, domain);
			}
		}

		public static Cities GetRussianCities()
		{
			return new Cities(Program.RussianCitiesFileName);
		}

		// here is a problem with non-unique city name
		public  string GetDomain(string place)
		{
			var city = place.Trim().ToLower();
			if(cities.ContainsKey(city))
				return cities[city];
			if (domains.ContainsKey(city))
				return domains[city];
			return null;
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
			Console.WriteLine(w.Count);
		}
	}
}
