using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cqa_medical.Utilits
{
	class DataActualityChecker
	{
		public static IEnumerable<T> Check<T>(Lazy<T[]> data,Func<T,string> formatStringWrite, Func<string,T> formatStringParse, FileDependencies dependencies)
		{

			if (!dependencies.IsFileActual())
			{
				var dataArray = data.Value.ToArray();
				Console.WriteLine("Generating " + dependencies.DestinationFile);
				File.WriteAllLines(dependencies.DestinationFile, dataArray.Select(formatStringWrite).ToArray());
				return dataArray;
			}
			Console.WriteLine("Taking from " + dependencies.DestinationFile);
			var lines = File.ReadAllLines(dependencies.DestinationFile);
			var parsedLines = lines.Select(formatStringParse);
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
		public bool IsFileActual()
		{
			return DataActualityChecker.IsFileActual(DestinationFile, DependencyFiles);
		}
		public FileDependencies(string destinationFile,  params string[] dependencyFiles)
		{
			DestinationFile = destinationFile;
			DependencyFiles = dependencyFiles ?? new string[] { };
		}
	}

}
