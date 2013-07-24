using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace
{
    public class OctaveMultiplePlots
    {
        private OctavePlot[] plots;

        public string Title;
        public string XLabel;
        public string YLabel;
        public bool GridVisible;

        private static readonly string someOtherCommands = "";

        private string GenerateAddScript()
        {
            return Title.EmptyOrFormat("title '{0}';\n") +
             XLabel.EmptyOrFormat("xlabel '{0}';\n") +
             YLabel.EmptyOrFormat("ylabel '{0}';\n") +
             (GridVisible ? "grid;\n" : "") +
             someOtherCommands;
        }

        /// <param name="linesByPoints">Each Tuple is a line represented by X-s and Y-s coordinates of its points and name of line</param>
        public OctaveMultiplePlots(IEnumerable<Tuple<double[], double[], string>> linesByPoints)
        {
            int i = 0;
            plots = linesByPoints.Select(
                l => new OctavePlot(l.Item1, l.Item2)
                         {
                             Style = PlotStyle.LineWithColor(((i++)%6).ToString()+';'+l.Item3+';')
                         }).ToArray();
        }

        public OctaveMultiplePlots(OctavePlot[] plots)
        {
            this.plots = plots;
        }


        /// <returns>message from Octave</returns>
        public string DrawPlotsTo(string fileToSave)
        {
            var script = "hold \n";
            foreach (var octavePlot in plots)
            {
                script += octavePlot.GenerateScript();
            }
            script += "hold\n";
            script += GenerateAddScript();
            script += OctavePlot.GenerateSaveScript(fileToSave);
            return OctaveController.Execute(script);
        }
    }

    public class OctavePlot
	{
		public PlotStyle Style = PlotStyle.Line;
		public string Title;
		public string XLabel;
		public string YLabel;
		public bool GridVisible;

		private readonly CultureInfo cul =  new CultureInfo("ru") { NumberFormat = { NumberDecimalSeparator = "." } };
		private readonly string dataX;
		private readonly string dataY;
		private readonly string someOtherCommands = "";

		public OctavePlot(double[] dataX, double[] dataY)
		{
			if (dataX.Count() != dataY.Count())
				throw new Exception("Data Arrays Count mismatch");
			this.dataX = Embrace(String.Join(",", dataX.Select(d => d.ToString(cul))));
			this.dataY = Embrace(String.Join(",", dataY.Select(d => d.ToString(cul))));
		}
		public OctavePlot( DateTime[] dataX, double[] dataY)
			: this( dataX.Select(t => (double)t.Ticks).ToArray(), dataY)
		{
			someOtherCommands = "datalabels={"+
				String.Join(",",GenerateDateXTicks(8,dataX).Select(s => "'"+s+"'"))+
				"};set(gca(),'xticklabel',datalabels);";
		}
		private static IEnumerable<string> GenerateDateXTicks(int howMany, DateTime[] dates)
		{
			var max = dates.Max();
			var min = dates.Min();
			var minInterval = (max - min).TotalSeconds / howMany;
			var result = new List<DateTime>();
			for (int i = 0; i < howMany; ++i)
			{
				result.Add(min.AddSeconds(minInterval * i));
			}

			return result.Select(s => s.ToString("yyyy-MM-dd")).ToArray();
		}

		public OctavePlot(Point[] points)
		{
			dataX = Embrace(String.Join(",", points.Select(d => d.X.ToString(cul))));
			dataY = Embrace(String.Join(",", points.Select(d => d.Y.ToString(cul))));
		}
		public OctavePlot(PointF[] points)
		{
			dataX = Embrace(String.Join(",", points.Select(d => d.X.ToString(cul))));
			dataY = Embrace(String.Join(",", points.Select(d => d.Y.ToString(cul))));
		}
		public string GenerateScript()
		{
		    return String.Format("{2}({0},{1}{3});\n", dataX, dataY, Style.PlotType, Style.Style) +
		           Title.EmptyOrFormat("title '{0}';\n") +
		           XLabel.EmptyOrFormat("xlabel '{0}';\n") +
		           YLabel.EmptyOrFormat("ylabel '{0}';\n") +
		           (GridVisible ? "grid;\n" : "") +
		           someOtherCommands;
		}
        public static string GenerateSaveScript(string fileToSave)
        {
            return String.Format("print -d{0} {1};\n", fileToSave.Substring(fileToSave.LastIndexOf('.') + 1), fileToSave);
        }

        /// <returns>message from Octave</returns>
        public string DrawPlotTo(string fileToSave)
        {
            var script = GenerateScript() + "\n" + GenerateSaveScript(fileToSave);
			return OctaveController.Execute(script);
		}

        private string Embrace(string s)
        {
            return "["+s+"]";
        }
	}

   
	public class PlotStyle
	{
		public static PlotStyle Line = new PlotStyle("plot", "'-'",  "'linewidth', 3");
		public static PlotStyle Dot = new PlotStyle("plot", "'o'",  "'markersize', 5");
		public static PlotStyle Bar = new PlotStyle("bar");
		public static PlotStyle Stairs = new PlotStyle("stairs");


        
        /// <param name="color">one of "k" (black), "r" (red), "g" (green), "b" (blue), "m" (magenta), "c" (cyan), or "w" (white)</param>
	    public static PlotStyle LineWithColor(string color)
	    {
	        return new PlotStyle("plot", "'-" + color + "'", "'linewidth', 3");
	    }

	    public static PlotStyle LineWithTrendLine(double[] dataX, double[] dataY, int count = 1)
		{
			Assert.AreEqual(dataX.Count(), dataY.Count());
			var countX2 = count * 2;
			var length = dataX.Length - count;
			Assert.Less(countX2, dataX.Length, "Lack of points" );

			var dataSmoothX = new List<double>();
			var dataSmoothY = new List<double>();
			var curAverageX = dataX.Take(countX2).Average();
			var curAverageY = dataY.Take(countX2).Average();
			dataSmoothX.Add(curAverageX);
			dataSmoothY.Add(curAverageY);
			for (int i = count; i < length; ++i)
			{
				curAverageX += dataX[i + count]/countX2 - dataX[i - count]/countX2;
				curAverageY += dataY[i + count] / countX2 - dataY[i - count] / countX2;
				dataSmoothX.Add(curAverageX);
				dataSmoothY.Add(curAverageY);
			}
			return new PlotStyle(
				"plot",
				String.Format(
					"[{0}],[{1}]",
					String.Join(",", dataSmoothX.Select(s => s.ToString(Cul))),
					String.Join(",", dataSmoothY.Select(s => s.ToString(Cul)))),
				"'r-', 'linewidth', 3"
				);
		}


		private static readonly CultureInfo Cul = new CultureInfo("ru") { NumberFormat = { NumberDecimalSeparator = "." } };
		public string PlotType{get { return plotType; }}

		public string Style
		{
			get
			{
				return firstModifier.EmptyOrFormat(", {0}") +
					secondModifier.EmptyOrFormat(", {0}");

			}
		}


		private readonly string plotType;
		private readonly string firstModifier;
		private readonly string secondModifier;

		private PlotStyle(string plotType, string firstModifier = "", string secondModifier = "")
		{
			this.plotType = plotType;
			this.firstModifier = firstModifier;
			this.secondModifier = secondModifier;
		}
	}

	[TestFixture]
	internal class PlotDrawerTest
	{
		[Test]
		public void PlotTest()
		{
			var dataX = new[] {1, 2, 3.3, 4.9, 5};
			var dataY = new[] {5.0, 7, 8, 3, 2};
			var q = new OctavePlot( dataX, dataY)
			        	{
							Style = PlotStyle.LineWithTrendLine(dataX, dataY), 
							XLabel = "линия снизу",
							YLabel = "Линия сбоку",
							Title = "trolo",
							GridVisible = true
			        	}.DrawPlotTo("1.png") ;

			Console.WriteLine(q);
		}
        [Test]
        public void PlotTest2()
        {
            var dataX = new[] {1, 2, 3.3, 4.9, 5};
            var dataY = new[] {5.0, 7, 8, 3, 2};
            var dataX2 = new[] {1, 2, 3.3, 4.9, 5};
            var dataY2 = new[] {4.0, 6, 10, 1, 8};
            var q = new OctaveMultiplePlots(new[]
                                                {
                                                    Tuple.Create(dataX, dataY, "first"),
                                                    Tuple.Create(dataX2, dataY2, "second")
                                                })
                        {
                            XLabel = "линия снизу",
                            YLabel = "Линия сбоку",
                            Title = "Название",
                            GridVisible = true
                        }
                .DrawPlotsTo("3m.png");
            Console.WriteLine(q);
        }

	    [Test]
		public void DatePlotTest()
		{
			var q = new OctavePlot(new[] {new DateTime(1,1,1), new DateTime(1,1,7) }, new[] {5.0, 7})
			        	{
							Style = PlotStyle.Bar, 
							XLabel = "линия снизу",
							YLabel = "Линия сбоку",
							Title = "trolo"
                        }.DrawPlotTo("2.png");

			Console.WriteLine(q);
		}
	}
}
