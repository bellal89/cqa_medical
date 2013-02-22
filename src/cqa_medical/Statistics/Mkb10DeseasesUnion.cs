using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LinqLib.Sequence;
using cqa_medical.UtilitsNamespace.Parsers;

namespace cqa_medical.Statistics
{
	public class Mkb10DeseasesUnion
	{
		private readonly Dictionary<string, int> deseaseToLine;
		private readonly List<Tuple<int, List<string>>> lineToDesease;
		private readonly Dictionary<int, HashSet<int>> joinedDeseases;

		// Joins some deseases marked by hands
		public Mkb10DeseasesUnion()
		{
			lineToDesease = ReadJoinedDeseasesFile(Program.FilesDirectory + "Mkb10-des-top100-joined.txt");
			deseaseToLine = lineToDesease.ToDictionary(it => it.Item2[0].Split('(')[0].Trim(), it => it.Item1);
			joinedDeseases = JoinDeseases(lineToDesease);
		}

		private static List<Tuple<int, List<string>>> ReadJoinedDeseasesFile(string joinedDeseasesFileName)
		{
			var lines = File.ReadAllLines(joinedDeseasesFileName).ToList();

			var lnToDesease = new List<Tuple<int, List<string>>>();
			for (var i = 1; i < lines.Count() + 1; i++)
			{
				var line = lines[i - 1];
				if (String.IsNullOrEmpty(line))
					continue;

				var parts = line.Split('\t');
				if (parts.Length < 2)
				{
					lnToDesease.Add(Tuple.Create(i, new List<string> {line}));
				}
				else
				{
					lnToDesease.Last().Item2.Add(parts[1]);
				}
			}
			return lnToDesease;
		}

		private static Dictionary<int, HashSet<int>> JoinDeseases(IEnumerable<Tuple<int, List<string>>> lnToDesease)
		{
			var joined = new Dictionary<int, HashSet<int>>();
			foreach (var lnToDes in lnToDesease)
			{
				var lineNumbers = GetLineNumbers(lnToDes.Item2[0]);
				if (!joined.ContainsKey(lnToDes.Item1))
				{
					if (lineNumbers.Any(joined.ContainsKey))
					{
						joined[lnToDes.Item1] = joined[lineNumbers.Where(joined.ContainsKey).First()];
					}
					else
					{
						joined[lnToDes.Item1] = new HashSet<int>();
					}
					joined[lnToDes.Item1].Add(lnToDes.Item1);
				}
				foreach (var ln in lineNumbers)
				{
					joined[ln] = joined[lnToDes.Item1];
					joined[ln].Add(ln);
				}
			}
			return joined;
		}

		private static List<int> GetLineNumbers(string deseaseName)
		{
			var match = Regex.Match(deseaseName, @".+\(((\d+,?\s*)+)\)");
			if (match.Success)
				return
					match.Groups[1].ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(
						ln => int.Parse(ln.Trim())).ToList();
			return new List<int>();
		}

		public List<Mkb10Desease> GetJoinedDeseases(ICollection<Mkb10Desease> deseases)
		{
			var nameToDesease = deseases.ToDictionary(des => des.Name.Trim(), des => des);
			var containedDeseases = deseases.Where(desease => nameToDesease.ContainsKey(desease.Name.Trim())).ToList();
			
			var newDeseases = new List<Mkb10Desease>();
			foreach (var desease in containedDeseases)
			{
				if (!nameToDesease.ContainsKey(desease.Name.Trim()))
					continue;
				newDeseases.Add(GetJoinedDesease(nameToDesease, desease));
			}
			return newDeseases;
		}

		private Mkb10Desease GetJoinedDesease(IDictionary<string, Mkb10Desease> deseases, Mkb10Desease desease)
		{
			var desName = desease.Name.Trim();
			if (!deseaseToLine.ContainsKey(desName))
				return desease;
			var line = deseaseToLine[desName];
			var joined = joinedDeseases[line];
			if (joined.Count == 1 && joined.First() == line)
				return desease;

			var lineToDes = lineToDesease.ToDictionary(it => it.Item1, it => it.Item2);
			var joinedDeseaseItems = joined.Select(ln => lineToDes[ln]).ToList();
			var desNames = joinedDeseaseItems.Where(des => des.Any()).Select(des => des[0].Split('(')[0].Trim()).ToList();
			var joinedDeseaseName = desNames.ElementAtMin(name => name.Length);
			var joinedDeseaseSynonyms = joinedDeseaseItems.SelectMany(des => des.Select(name => name.Split('(')[0].Trim())).Distinct().ToList();

			var newDesease = new Mkb10Desease(joinedDeseaseName, desease.Parent,
			                                     desNames.Where(deseases.ContainsKey).SelectMany(
			                                     	name => deseases[name].TradeNames).ToList(), joinedDeseaseSynonyms);
			foreach(var name in desNames)
			{
				deseases.Remove(name);
			}
			return newDesease;
		}
	}
}