using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;
using NUnit.Framework;
using cqa_medical.BodyAnalisys;

namespace cqa_medical.Utilits
{
	class FileActualityChecker
	{
//		public static IEnumerable<T> Check<T>(char delimeter, Func< IEnumerable<T>> getData, FileDependencies dependencies) where T : class
//		{
//			var configs = new CsvConfiguration()
//			              	{
//								Delimiter = delimeter,
//			              		Quote = (char) 1,
//			              		HasHeaderRecord = false,
//			              		UseInvariantCulture = true
//			              	};
//			if (!IsFileActual(dependencies.DestinationFile, dependencies.DependencyFiles))
//			{
//				Console.WriteLine("Generating from " + dependencies.SourceFile);
//				var data = getData().ToArray();
//				using (var writeStream = new StreamWriter(dependencies.DestinationFile))
//				{
//					var writer = new CsvHelper.CsvWriter(writeStream, configs);
//					writer.WriteRecords(data);
//				}
//				return data;
//			}
//			using (var readStream = new StringReader(dependencies.DestinationFile))
//			{
//				var csvReader = new CsvHelper.CsvReader(readStream, configs);
//				var enumerable = csvReader.GetRecords<T>();
//				var q =  enumerable.ToArray();
//				return q;
//			}
//		}
		public static IEnumerable<T> Check<T>(Func<IEnumerable<T>> getData, FileDependencies dependencies) where T : IFormatParse<T>, new()
		{
			if (!IsFileActual(dependencies.DestinationFile, dependencies.DependencyFiles))
			{
				Console.WriteLine("Generating " + dependencies.DestinationFile);
				var data = getData().ToArray();
				File.WriteAllLines(dependencies.DestinationFile, data.Select(s => s.FormatStringWrite()).ToArray());
				return data;
			}
			var lines = File.ReadAllLines(dependencies.DestinationFile);
			var parsedLines = lines.Select(s => (new T()).FormatStringParse(s));
			return parsedLines;
		}
		public static bool IsFileActual(string fileName, string[] parentFileNames)
		{
			return File.Exists(fileName) && parentFileNames.All(parentFileName =>
				File.GetLastWriteTime(parentFileName) < File.GetCreationTime(fileName));
		}
	}

	internal class FileDependencies
	{
		public string DestinationFile;
		public string[] DependencyFiles;

		public FileDependencies(string destinationFile,  params string[] dependencyFiles)
		{
			DestinationFile = destinationFile;
			DependencyFiles = dependencyFiles ?? new string[] { };
		}
	}

}
