using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace
{
	/// <summary>
	/// Executes Octave scripts (Matlab compatible product).
	/// </summary>
	public class OctaveController
	{
		public static bool NoGUI = true;
		private static string executable = @"c:\Octave\3.2.4_gcc-4.4.0\bin\octave.exe";

		public static string FunctionSearchPath;

		/// <summary>
		/// Example script="A=[1 2]; B=[3; 4]; C=A*B";
		/// --eval is limitted to Windows command line buffer, so this is why temporary file is used for the input script 
		/// </summary>
		/// <param name="script"></param>
		/// <returns>result from Octave</returns>
		public static string Execute(string script)
		{
			string output = "";
			string tempFile = "";
			string error = "";
			try
			{
				script = "addpath('" + FunctionSearchPath + "');\r\n" + script;
					//addpath (genpath ("~/octave")); //for recursive search
				tempFile = SaveTempFile(script);
				string param = tempFile;

				System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(executable, param)
				                                          	{
				                                          		RedirectStandardOutput = true,
				                                          		RedirectStandardInput = true,
				                                          		RedirectStandardError = true
				                                          	};


				if (NoGUI)
				{
					psi.CreateNoWindow = true;
					psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				}
				else
					psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

				psi.UseShellExecute = false;

				System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);
				output = p.StandardOutput.ReadToEnd();
				error = "  " + p.StandardError.ReadToEnd();
				if (error.IndexOf("error", System.StringComparison.Ordinal) > -1 )
					throw new Exception(ParseResult(output) + "\n" + error);

			}
				//catch (Exception ex)
				//{
				//    Console.WriteLine(ex.Message);
				//}
			finally
			{
				if (File.Exists(tempFile))
					File.Delete(tempFile);
			}

			return ParseResult(output) + error;
		}

		private static string ParseResult(string output)
		{
			int pos = output.IndexOf("`news'.");

			return (pos != -1) ? output.Substring(pos + 7) : output;
		}

		public static string SaveTempFile(string conent)
		{
			string tempFile = Path.GetTempFileName();

			using (StreamWriter outfile =
				new StreamWriter(tempFile))
			{
				outfile.Write(conent);
			}

			return tempFile;
		}

		public static string SaveTempFile(List<double[]> values)
		{
			string tempFile = Path.GetTempFileName();

			StreamWriter outfile = new StreamWriter(tempFile);
			try
			{
				foreach (var row in values)
				{
					for (int i = 0; i < row.Length; i++)
					{
						if (i == row.Length - 1) outfile.WriteLine(row[i]);
						else outfile.Write(row[i] + " ");
					}
				}

			}
			finally
			{
				if (outfile != null)
				{
					outfile.Close();
					outfile = null;
				}
			}

			return tempFile;
		}
	}

	

	[TestFixture]
	internal class OctaveTest
	{
		
		[Test]
		public  void Qwe()
		{
			var q = OctaveController.Execute(@"
			x = [1,2,3,4,5,6,7,8,9,10];
			y = [3,4,5,6,7,1,8,9,4,5];
			stairs(x,y);
			print -dpng 1.png;			                                                                         
			");
			Console.WriteLine(q);
		}
		
	}
}
