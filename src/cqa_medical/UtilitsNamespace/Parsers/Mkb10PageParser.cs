using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace.Parsers
{
	class Mkb10PageParser : HTMLPageParser<Mkb10Desease>
	{
		private readonly HtmlDocument html = new HtmlDocument();
		
		public Mkb10PageParser(string pagesDirectory) : base(pagesDirectory)
		{
		}

		protected override Mkb10Desease ParsePage(string fileName)
		{
			html.Load(fileName, Encoding.GetEncoding(1251));
			var breadcrumbs = html.GetElementbyId("breadcrumbs");
			if (breadcrumbs == null)
			{
				Console.WriteLine("Breadcrumbs not found: " + fileName);
				return null;
			}
			var name = breadcrumbs.LastChild.InnerText.Trim().ToLower();
			if (name.StartsWith("&gt;")) name = name.Remove(0, 4).TrimStart();
			var content = html.GetElementbyId("content");
			var tradeNodes = content.SelectNodes("table[@class='rest_nest']/tr/td[@class='rest_data']");
			if (tradeNodes == null)
			{
				Console.WriteLine("Trade names not found.");
				Console.WriteLine("File: " + fileName);
				Console.WriteLine("Name: " + name);
				return null;
			}
			var tradeNames = new List<Mkb10TradeName>();
			for (var i = 0; i < tradeNodes.Count - 1; i += 2)
			{
				var tradeName = tradeNodes[i].ChildNodes.FindFirst("a");
				if (tradeName == null) continue;
				var tradeSubstance = tradeNodes[i+1].ChildNodes.FindFirst("a");
				tradeNames.Add(new Mkb10TradeName(tradeName.InnerText.ToLower().Trim().TrimEnd('®'), tradeSubstance != null ? tradeSubstance.InnerText.ToLower().Trim().TrimEnd('®') : ""));
			}

			var synonymsNode = html.GetElementbyId("synonyms");
			return synonymsNode == null
			       	? new Mkb10Desease(name, tradeNames)
			       	: new Mkb10Desease(name, tradeNames, synonymsNode.SelectNodes("ul/li").Select(li => li.InnerText.Trim().ToLower()).ToList());
		}
	}

	[Serializable]
	internal class Mkb10Desease
	{
		public string Name { get; set; }
		public readonly List<Mkb10TradeName> TradeNames;
		public readonly List<string> Synonyms; 

		public Mkb10Desease(string name, List<Mkb10TradeName> tradeNames)
		{
			Name = name;
			TradeNames = tradeNames;
			Synonyms = new List<string>();
		}

		public Mkb10Desease(string name, List<Mkb10TradeName> tradeNames, List<string> synonyms)
		{
			Name = name;
			TradeNames = tradeNames;
			Synonyms = synonyms;
		}
	}

	[Serializable]
	internal class Mkb10TradeName
	{
		public string Name { get; set; }
		public string ActiveSubstance { get; set; }
		public Mkb10TradeName(string name, string activeSubstance)
		{
			Name = name;
			ActiveSubstance = activeSubstance;
		}
	}

	[TestFixture]
	public class Mkb10Test
	{
		[Test]
		public static void TestCreation()
		{
			var parser = new Mkb10PageParser(Program.FilesDirectory + "Mkb10/");
			var mkb10Deseases = parser.GetObjects();
		}
	}
}
