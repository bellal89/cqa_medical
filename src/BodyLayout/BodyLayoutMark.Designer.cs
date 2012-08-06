namespace BodyLayout
{
	partial class BodyLayoutMark
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BodyPanel = new System.Windows.Forms.Panel();
			this.BodyPartsTable = new System.Windows.Forms.DataGridView();
			this.BodyPart = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Coordinates = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.StripMenu = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.BodyPartsTable)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BodyPanel
			// 
			this.BodyPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.BodyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.BodyPanel.Cursor = System.Windows.Forms.Cursors.Cross;
			this.BodyPanel.Location = new System.Drawing.Point(12, 24);
			this.BodyPanel.Name = "BodyPanel";
			this.BodyPanel.Size = new System.Drawing.Size(266, 303);
			this.BodyPanel.TabIndex = 0;
			this.BodyPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.BodyPanel_MouseClick);
			this.BodyPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BodyPanel_MouseDown);
			// 
			// BodyPartsTable
			// 
			this.BodyPartsTable.AllowUserToResizeColumns = false;
			this.BodyPartsTable.AllowUserToResizeRows = false;
			this.BodyPartsTable.BackgroundColor = System.Drawing.SystemColors.ControlLight;
			this.BodyPartsTable.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.BodyPartsTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.BodyPartsTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.BodyPart,
            this.Coordinates});
			this.BodyPartsTable.Dock = System.Windows.Forms.DockStyle.Right;
			this.BodyPartsTable.Location = new System.Drawing.Point(315, 24);
			this.BodyPartsTable.MultiSelect = false;
			this.BodyPartsTable.Name = "BodyPartsTable";
			this.BodyPartsTable.Size = new System.Drawing.Size(325, 316);
			this.BodyPartsTable.TabIndex = 1;
			this.BodyPartsTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BodyPartsTable_CellClick);
			this.BodyPartsTable.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.BodyPartsTable_CellMouseDown);
			// 
			// BodyPart
			// 
			this.BodyPart.HeaderText = "Часть Тела";
			this.BodyPart.Name = "BodyPart";
			this.BodyPart.Width = 180;
			// 
			// Coordinates
			// 
			this.Coordinates.HeaderText = "Координаты";
			this.Coordinates.Name = "Coordinates";
			this.Coordinates.ReadOnly = true;
			this.Coordinates.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StripMenu});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(640, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// StripMenu
			// 
			this.StripMenu.Name = "StripMenu";
			this.StripMenu.Size = new System.Drawing.Size(45, 20);
			this.StripMenu.Text = "Load";
			this.StripMenu.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
			// 
			// BodyLayoutMark
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(640, 340);
			this.Controls.Add(this.BodyPartsTable);
			this.Controls.Add(this.BodyPanel);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "BodyLayoutMark";
			this.Text = "BodyLayoutMark";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BodyLayoutMark_FormClosing);
			this.Load += new System.EventHandler(this.BodyLayoutMark_Load);
			((System.ComponentModel.ISupportInitialize)(this.BodyPartsTable)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel BodyPanel;
		private System.Windows.Forms.DataGridView BodyPartsTable;
		private System.Windows.Forms.DataGridViewTextBoxColumn BodyPart;
		private System.Windows.Forms.DataGridViewTextBoxColumn Coordinates;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem StripMenu;
	}
}

