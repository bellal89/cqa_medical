﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using NUnit.Framework;

namespace cqa_medical.UtilitsNamespace
{
	public class OctavePlot
	{
		public PlotStyle Style = PlotStyle.Line;
		public string Title;
		public string XLabel;
		public string YLabel;
		public bool GridVisible;



		private readonly CultureInfo cul =  new CultureInfo("ru") { NumberFormat = { NumberDecimalSeparator = "." } };
		private readonly string fileToSave;
		private readonly string dataX;
		private readonly string dataY;
		private readonly string someOtherCommands = "";

		public OctavePlot(string fileToSave, double[] dataX, double[] dataY)
		{
			this.fileToSave = fileToSave;
			if (dataX.Count() != dataY.Count())
				throw new Exception("Data Arrays Count mismatch");
			this.dataX = String.Join(",", dataX.Select(d => d.ToString(cul)));
			this.dataY = String.Join(",", dataY.Select(d => d.ToString(cul)));
		}
		public OctavePlot(string fileToSave, DateTime[] dataX, double[] dataY)
			: this(fileToSave, dataX.Select(t => (double)t.Ticks).ToArray(), dataY)
		{
			someOtherCommands = "datalabels={"+
				String.Join(",",GenerateDateXTicks(8,dataX).Select(s => "'"+s+"'"))+
				"};set(gca(),'xticklabel',datalabels);";
		}

		private string[] GenerateDateXTicks(int howMany, DateTime[] dates)
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

		public OctavePlot(string fileToSave, Point[] points)
		{
			this.fileToSave = fileToSave;
			dataX = String.Join(",", points.Select(d => d.X.ToString(cul)));
			dataY = String.Join(",", points.Select(d => d.Y.ToString(cul)));
		}
		public OctavePlot(string fileToSave, PointF[] points)
		{
			this.fileToSave = fileToSave;
			dataX = String.Join(",", points.Select(d => d.X.ToString(cul)));
			dataY = String.Join(",", points.Select(d => d.Y.ToString(cul)));
		}
		
		public string DrawPlot()
		{
			string script =
				String.Format("{2}([{0}],[{1}] {3});",dataX,dataY, Style.PlotType, Style.Style) +
				(String.IsNullOrEmpty(Title) ? "" : String.Format("title '{0}';", Title)) +
				(String.IsNullOrEmpty(XLabel) ? "" : String.Format("xlabel '{0}';", XLabel)) +
				(String.IsNullOrEmpty(YLabel) ? "" : String.Format("ylabel '{0}';", YLabel)) +
				(GridVisible? "grid;":"") +
				someOtherCommands +
				String.Format("print -d{0} {1}", fileToSave.Substring(fileToSave.LastIndexOf('.') + 1), fileToSave);

//			Console.WriteLine(script);
			return OctaveController.Execute(script);
		}
	}
	
	public class PlotStyle
	{
		public static PlotStyle Line = new PlotStyle("plot", ", '-'", ", 'linewidth', 3");
		public static PlotStyle Dot = new PlotStyle("plot", ", 'o1'",", 'markersize', 5");
		public static PlotStyle Bar = new PlotStyle("bar");
		public static PlotStyle Stairs = new PlotStyle("stairs");




		public string PlotType{get { return plotType; }}
		public string Style { get { return style + width; } }


		private readonly string plotType;
		private readonly string style;
		private readonly string width;

		private PlotStyle(string plotType, string style = "", string width = "")
		{
			this.plotType = plotType;
			this.style = style;
			this.width = width;
		}
	}

	[TestFixture]
	internal class PlotDrawerTest
	{
		[Test]
		public void PlotTest()
		{
			var q = new OctavePlot("1.png", new[] {1, 2, 3.3, 4.9, 5}, new[] {5.0, 7, 8, 3, 2})
			        	{
							Style = PlotStyle.Bar, 
							XLabel = "линия снизу",
							YLabel = "Линия сбоку",
							Title = "trolo",
							GridVisible = true
			        	}.DrawPlot() ;

			Console.WriteLine(q);
		}
		[Test]
		public void DatePlotTest()
		{
			var q = new OctavePlot("2.png", new[] {new DateTime(1,1,1), new DateTime(1,1,7) }, new[] {5.0, 7})
			        	{
							Style = PlotStyle.Bar, 
							XLabel = "линия снизу",
							YLabel = "Линия сбоку",
							Title = "trolo",
							GridVisible = true
			        	}.DrawPlot() ;

			Console.WriteLine(q);
		}
	}
}
