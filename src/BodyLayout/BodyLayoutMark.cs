using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace BodyLayout
{
	public partial class BodyLayoutMark : Form
	{
		private const char DelimeterC = '\x2';
		private const string DelimeterS = "\x2";

		private Rectangle circle;
		private Graphics graphics;

		private string pictureFileName;
		private string bodyPartCoordinatesFileName;
		private bool initialized;

		public BodyLayoutMark()
		{
			InitializeComponent();
		}
		private void InitPicture(string fileName)
		{
			pictureFileName = fileName;
			bodyPartCoordinatesFileName = fileName + ".BodyPartsCoordinates.txt";
			var image = Image.FromFile(pictureFileName);
			BodyPanel.Size = image.Size;
			BodyPanel.BackgroundImage = image;
			initialized = true;
			graphics = BodyPanel.CreateGraphics();
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			Size = BodyPanel.Size + new Size(BodyPart.Width, 0) + new Size(200 , 80);
			LoadDataGrid();
			Activate();
		}
		private static string Row2String(DataGridViewRow row)
		{
			var cells = new[] { (string)row.Cells[0].Value, (string)row.Cells[1].Value };
			if (cells.All(String.IsNullOrEmpty)) return "";
			return String.Join(DelimeterS, cells);
		}
		private DataGridViewRow ActiveRow()
		{
			return BodyPartsTable.Rows[BodyPartsTable.SelectedCells[0].RowIndex];
		}

		private void EraseCircle()
		{
			BodyPanel.Invalidate(new Rectangle(circle.Location, circle.Size + new Size(2, 2)));
		}
		private void DrawCircle(Point loc)
		{
			circle = new Rectangle(loc, new Size(25, 25));
			graphics.FillEllipse(new SolidBrush(Color.Crimson), circle);
		}
		private void SaveCoordinates(DataGridViewRow row, Point loc)
		{
			row.Cells[1].Value = loc.ToString();
		}
		private void LoadDataGrid()
		{
			if (!File.Exists(bodyPartCoordinatesFileName))
				File.Create(bodyPartCoordinatesFileName);
			var lines = File.ReadAllLines(bodyPartCoordinatesFileName);
			var rows = BodyPartsTable.Rows;
			rows.Clear();
			foreach (var t in lines.Where(s => s != ""))
			{
				rows.Add(t.Split(DelimeterC));
			}
			BodyPartsTable.Rows[0].Selected = true;
		}
		private void SaveDataGrid()
		{
			var lines = new List<string>();
			var rows = BodyPartsTable.Rows;
			for (var i = 0; i < rows.Count; ++i)
			{
				lines.Add(Row2String(rows[i]));
			}
			File.WriteAllLines(bodyPartCoordinatesFileName, lines);
		}

		private void BodyLayoutMark_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveDataGrid();
		}

		private void BodyLayoutMark_Load(object sender, EventArgs e)
		{
			if (!initialized) return;
			LoadDataGrid();
		}

		private void BodyPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (!initialized) return;
			var loc = e.Location - (Size)BodyPanel.Location + new Size(-1,11);
			DrawCircle(loc);

			SaveCoordinates(ActiveRow(), circle.Location);

		}
		private void BodyPanel_MouseDown(object sender, MouseEventArgs e)
		{
			if (!initialized) return;
			EraseCircle();
		}

		private void BodyPartsTable_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (!initialized) return;
			var rowCells = ActiveRow().Cells;
			if (rowCells.Count <= 0) return;
			var value = (string)rowCells[1].Value;
			if (value == null) return;
			var match = Regex.Match(value, @"{X=(?<x>\d+),Y=(?<y>\d+)}");
			if (!match.Success) return;
			DrawCircle(new Point(int.Parse(match.Groups["x"].Value), int.Parse(match.Groups["y"].Value)));
		}

		private void BodyPartsTable_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (!initialized) return;
			EraseCircle();
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var fileDialog = new OpenFileDialog
			                      	{
			                      		Filter = "All files (*.*)|*.*",
			                      		RestoreDirectory = true,
										Multiselect = false
			                      	};
			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				if (initialized)
				{
					SaveDataGrid();
				}
				InitPicture( fileDialog.FileName);
				initialized = true;
			}
		}

	}
}
