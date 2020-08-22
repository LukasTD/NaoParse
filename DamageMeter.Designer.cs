using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace NaoParse
{
	partial class FrmDpsMeter
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDpsMeter));
			this.encounterDataGridView = new System.Windows.Forms.DataGridView();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.resetBttn = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.opacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			((System.ComponentModel.ISupportInitialize)(this.encounterDataGridView)).BeginInit();
			this.panel1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// encounterDataGridView
			// 
			this.encounterDataGridView.AllowUserToAddRows = false;
			this.encounterDataGridView.AllowUserToDeleteRows = false;
			this.encounterDataGridView.AllowUserToResizeRows = false;
			this.encounterDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
			this.encounterDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.encounterDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.encounterDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.encounterDataGridView.Location = new System.Drawing.Point(0, 24);
			this.encounterDataGridView.Name = "encounterDataGridView";
			this.encounterDataGridView.ReadOnly = true;
			this.encounterDataGridView.RowHeadersVisible = false;
			this.encounterDataGridView.RowTemplate.ReadOnly = true;
			this.encounterDataGridView.ShowCellErrors = false;
			this.encounterDataGridView.ShowCellToolTips = false;
			this.encounterDataGridView.ShowEditingIcon = false;
			this.encounterDataGridView.ShowRowErrors = false;
			this.encounterDataGridView.Size = new System.Drawing.Size(403, 600);
			this.encounterDataGridView.TabIndex = 1;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BackColor = System.Drawing.SystemColors.Desktop;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.resetBttn);
			this.panel1.Controls.Add(this.label1);
			this.panel1.ForeColor = System.Drawing.Color.White;
			this.panel1.Location = new System.Drawing.Point(0, 595);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(403, 29);
			this.panel1.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(72, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(147, 17);
			this.label2.TabIndex = 2;
			this.label2.Text = "- 100,000,000 DMG";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// resetBttn
			// 
			this.resetBttn.BackColor = System.Drawing.Color.Snow;
			this.resetBttn.Dock = System.Windows.Forms.DockStyle.Right;
			this.resetBttn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.resetBttn.ForeColor = System.Drawing.Color.Black;
			this.resetBttn.Location = new System.Drawing.Point(326, 0);
			this.resetBttn.Name = "resetBttn";
			this.resetBttn.Size = new System.Drawing.Size(75, 27);
			this.resetBttn.TabIndex = 1;
			this.resetBttn.Text = "Reset";
			this.resetBttn.UseVisualStyleBackColor = false;
			this.resetBttn.Click += new System.EventHandler(this.resetBttn_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "00:00:00";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// menuStrip1
			// 
			this.menuStrip1.BackColor = System.Drawing.SystemColors.Window;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logToolStripMenuItem,
            this.windowToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.menuStrip1.Size = new System.Drawing.Size(403, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// logToolStripMenuItem
			// 
			this.logToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem});
			this.logToolStripMenuItem.Name = "logToolStripMenuItem";
			this.logToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.logToolStripMenuItem.Text = "Log";
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
			this.exportToolStripMenuItem.Text = "Copy To Clipboard";
			this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
			// 
			// windowToolStripMenuItem
			// 
			this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysOnTopToolStripMenuItem,
            this.opacityToolStripMenuItem});
			this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
			this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
			this.windowToolStripMenuItem.Text = "Window";
			// 
			// alwaysOnTopToolStripMenuItem
			// 
			this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
			this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.alwaysOnTopToolStripMenuItem.Text = "Always On Top";
			this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
			// 
			// opacityToolStripMenuItem
			// 
			this.opacityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox1});
			this.opacityToolStripMenuItem.Name = "opacityToolStripMenuItem";
			this.opacityToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.opacityToolStripMenuItem.Text = "Opacity";
			// 
			// toolStripComboBox1
			// 
			this.toolStripComboBox1.Items.AddRange(new object[] {
            "70",
            "60",
            "50",
            "40",
            "30",
            "20",
            "10",
            "0"});
			this.toolStripComboBox1.Name = "toolStripComboBox1";
			this.toolStripComboBox1.Size = new System.Drawing.Size(121, 23);
			this.toolStripComboBox1.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			// 
			// FrmDpsMeter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(403, 624);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.encounterDataGridView);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FrmDpsMeter";
			this.Text = "Nao Parse";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmDpsMeter_FormClosed);
			this.Load += new System.EventHandler(this.FrmDpsMeter_Load);
			((System.ComponentModel.ISupportInitialize)(this.encounterDataGridView)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView encounterDataGridView;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button resetBttn;
		private System.Windows.Forms.Label label2;
		private MenuStrip menuStrip1;
		private ToolStripMenuItem windowToolStripMenuItem;
		private ToolStripMenuItem alwaysOnTopToolStripMenuItem;
		private ToolStripMenuItem opacityToolStripMenuItem;
		private ToolStripComboBox toolStripComboBox1;
		private ToolStripMenuItem logToolStripMenuItem;
		private ToolStripMenuItem exportToolStripMenuItem;
		private Timer timer1;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
	}
}