using ICSharpCode.TextEditor;
using System.Configuration;

namespace OrcaSql.OSMS
{
	partial class Main
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDatabaseDialog = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.treeview = new System.Windows.Forms.TreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.grid = new OrcaSql.OSMS.DataGridViewDoubleBuffered();
            this.txtCode = new ICSharpCode.TextEditor.TextEditorControl();
            this.gridStatus = new System.Windows.Forms.StatusStrip();
            this.gridStatusRows = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableMenu = new System.Windows.Forms.ContextMenu();
            this.showTableTopNRowsMenuItem = new System.Windows.Forms.MenuItem();
            this.exportDataMenuItem = new System.Windows.Forms.MenuItem();
            this.dmvMenu = new System.Windows.Forms.ContextMenu();
            this.showDmvRowsMenuItem = new System.Windows.Forms.MenuItem();
            this.baseTableMenu = new System.Windows.Forms.ContextMenu();
            this.baseTableSelectAllRowsMenuItem = new System.Windows.Forms.MenuItem();
            this.procedureMenu = new System.Windows.Forms.ContextMenu();
            this.showProcedureCodeMenuItem = new System.Windows.Forms.MenuItem();
            this.viewMenu = new System.Windows.Forms.ContextMenu();
            this.showViewCodeMenuItem = new System.Windows.Forms.MenuItem();
            this.functionMenu = new System.Windows.Forms.ContextMenu();
            this.showFunctionCodeMenuItem = new System.Windows.Forms.MenuItem();
            this.tableFunctionMenu = new System.Windows.Forms.ContextMenu();
            this.showTableFunctionCodeMenuItem = new System.Windows.Forms.MenuItem();
            this.tableTypeMenu = new System.Windows.Forms.ContextMenu();
            this.showTableTypeCodeMenuItem = new System.Windows.Forms.MenuItem();
            this.dataTypeMenu = new System.Windows.Forms.ContextMenu();
            this.showDataTypeCodeMenuItem = new System.Windows.Forms.MenuItem();
            this.databaseMenu = new System.Windows.Forms.ContextMenu();
            this.generateRestorePageScriptMenuItem = new System.Windows.Forms.MenuItem();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.gridStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(648, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem1,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.openToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem1
            // 
            this.openToolStripMenuItem1.Name = "openToolStripMenuItem1";
            this.openToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem1.Size = new System.Drawing.Size(206, 22);
            this.openToolStripMenuItem1.Text = "Open Database...";
            this.openToolStripMenuItem1.Click += new System.EventHandler(this.OpenToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(203, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // openDatabaseDialog
            // 
            this.openDatabaseDialog.AddExtension = false;
            this.openDatabaseDialog.Filter = "SQL Server data files|*.mdf;*.ndf";
            this.openDatabaseDialog.Multiselect = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.treeview);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 453);
            this.panel1.TabIndex = 3;
            // 
            // treeview
            // 
            this.treeview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeview.HideSelection = false;
            this.treeview.Location = new System.Drawing.Point(0, 0);
            this.treeview.Name = "treeview";
            this.treeview.Size = new System.Drawing.Size(200, 453);
            this.treeview.TabIndex = 0;
            this.treeview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseUp);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(200, 24);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 453);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.gridStatus);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(203, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(445, 453);
            this.panel2.TabIndex = 5;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.grid);
            this.panel3.Controls.Add(this.txtCode);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(445, 431);
            this.panel3.TabIndex = 3;
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.Size = new System.Drawing.Size(443, 429);
            this.grid.TabIndex = 4;
            this.grid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.Grid_CellFormatting);
            this.grid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.Grid_DataBindingComplete);
            // 
            // txtCode
            // 
            this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCode.Highlighting = "SQL";
            this.txtCode.Location = new System.Drawing.Point(0, 0);
            this.txtCode.Name = "txtCode";
            this.txtCode.ReadOnly = true;
            this.txtCode.ShowVRuler = false;
            this.txtCode.Size = new System.Drawing.Size(443, 429);
            this.txtCode.TabIndex = 3;
            // 
            // gridStatus
            // 
            this.gridStatus.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.gridStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gridStatusRows});
            this.gridStatus.Location = new System.Drawing.Point(0, 431);
            this.gridStatus.Name = "gridStatus";
            this.gridStatus.Size = new System.Drawing.Size(445, 22);
            this.gridStatus.TabIndex = 1;
            this.gridStatus.Text = "statusStrip2";
            // 
            // gridStatusRows
            // 
            this.gridStatusRows.Name = "gridStatusRows";
            this.gridStatusRows.Size = new System.Drawing.Size(0, 17);
            // 
            // tableMenu
            // 
            this.tableMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showTableTopNRowsMenuItem,
            this.exportDataMenuItem});
            // 
            // showTableTopNRowsMenuItem
            // 
            this.showTableTopNRowsMenuItem.Index = 0;
            this.showTableTopNRowsMenuItem.Text = "Select Top 1000 Rows";
            this.showTableTopNRowsMenuItem.Click += new System.EventHandler(this.ShowTableTopNRowsMenuItem_Click);
            // 
            // exportDataMenuItem
            // 
            this.exportDataMenuItem.Index = 1;
            this.exportDataMenuItem.Text = "Export Data";
            this.exportDataMenuItem.Click += new System.EventHandler(this.ExportDataMenuItem_Click);
            // 
            // dmvMenu
            // 
            this.dmvMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showDmvRowsMenuItem});
            // 
            // showDmvRowsMenuItem
            // 
            this.showDmvRowsMenuItem.Index = 0;
            this.showDmvRowsMenuItem.Text = "Select All Rows";
            this.showDmvRowsMenuItem.Click += new System.EventHandler(this.ShowDmvRowsMenuItem_Click);
            // 
            // baseTableMenu
            // 
            this.baseTableMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.baseTableSelectAllRowsMenuItem});
            // 
            // baseTableSelectAllRowsMenuItem
            // 
            this.baseTableSelectAllRowsMenuItem.Index = 0;
            this.baseTableSelectAllRowsMenuItem.Text = "Select All Rows";
            this.baseTableSelectAllRowsMenuItem.Click += new System.EventHandler(this.BaseTableSelectAllRowsMenuItem_Click);
            // 
            // procedureMenu
            // 
            this.procedureMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showProcedureCodeMenuItem});
            // 
            // showProcedureCodeMenuItem
            // 
            this.showProcedureCodeMenuItem.Index = 0;
            this.showProcedureCodeMenuItem.Text = "View Code";
            this.showProcedureCodeMenuItem.Click += new System.EventHandler(this.ShowProcedureCodeMenuItem_Click);
            // 
            // viewMenu
            // 
            this.viewMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showViewCodeMenuItem});
            // 
            // showViewCodeMenuItem
            // 
            this.showViewCodeMenuItem.Index = 0;
            this.showViewCodeMenuItem.Text = "View Code";
            this.showViewCodeMenuItem.Click += new System.EventHandler(this.ShowViewCodeMenuItem_Click);
            // 
            // functionMenu
            // 
            this.functionMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showFunctionCodeMenuItem});
            // 
            // showFunctionCodeMenuItem
            // 
            this.showFunctionCodeMenuItem.Index = 0;
            this.showFunctionCodeMenuItem.Text = "View Code";
            this.showFunctionCodeMenuItem.Click += new System.EventHandler(this.ShowFunctionCodeMenuItem_Click);
            // 
            // tableFunctionMenu
            // 
            this.tableFunctionMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showTableFunctionCodeMenuItem});
            // 
            // showTableFunctionCodeMenuItem
            // 
            this.showTableFunctionCodeMenuItem.Index = 0;
            this.showTableFunctionCodeMenuItem.Text = "View Code";
            this.showTableFunctionCodeMenuItem.Click += new System.EventHandler(this.ShowTableFunctionCodeMenuItem_Click);
            // 
            // tableTypeMenu
            // 
            this.tableTypeMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showTableTypeCodeMenuItem});
            // 
            // showTableTypeCodeMenuItem
            // 
            this.showTableTypeCodeMenuItem.Index = 0;
            this.showTableTypeCodeMenuItem.Text = "View Code";
            this.showTableTypeCodeMenuItem.Click += new System.EventHandler(this.ShowTableTypeCodeMenuItem_Click);
            // 
            // dataTypeMenu
            // 
            this.dataTypeMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showDataTypeCodeMenuItem});
            // 
            // showDataTypeCodeMenuItem
            // 
            this.showDataTypeCodeMenuItem.Index = 0;
            this.showDataTypeCodeMenuItem.Text = "View Code";
            this.showDataTypeCodeMenuItem.Click += new System.EventHandler(this.ShowDataTypeCodeMenuItem_Click);
            // 
            // databaseMenu
            // 
            this.databaseMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.generateRestorePageScriptMenuItem});
            // 
            // generateRestorePageScriptMenuItem
            // 
            this.generateRestorePageScriptMenuItem.Index = 0;
            this.generateRestorePageScriptMenuItem.Text = "Generate Restore Page script";
            this.generateRestorePageScriptMenuItem.Click += new System.EventHandler(this.GenerateRestorePageScriptMenuItem_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 477);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.Text = "Orca SQL Management  Studio";
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.gridStatus.ResumeLayout(false);
            this.gridStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openDatabaseDialog;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TreeView treeview;
		private System.Windows.Forms.ContextMenu tableMenu;
		private System.Windows.Forms.MenuItem showTableTopNRowsMenuItem;
		private System.Windows.Forms.ContextMenu dmvMenu;
		private System.Windows.Forms.MenuItem showDmvRowsMenuItem;
		private System.Windows.Forms.ContextMenu baseTableMenu;
		private System.Windows.Forms.MenuItem baseTableSelectAllRowsMenuItem;
		private System.Windows.Forms.StatusStrip gridStatus;
		private System.Windows.Forms.ToolStripStatusLabel gridStatusRows;
		private System.Windows.Forms.ContextMenu procedureMenu;
		private System.Windows.Forms.MenuItem showProcedureCodeMenuItem;
		private System.Windows.Forms.ContextMenu viewMenu;
		private System.Windows.Forms.MenuItem showViewCodeMenuItem;
        private System.Windows.Forms.MenuItem exportDataMenuItem;
        private System.Windows.Forms.ContextMenu functionMenu;
        private System.Windows.Forms.MenuItem showFunctionCodeMenuItem;
        private System.Windows.Forms.ContextMenu tableFunctionMenu;
        private System.Windows.Forms.MenuItem showTableFunctionCodeMenuItem;
        private System.Windows.Forms.ContextMenu tableTypeMenu;
        private System.Windows.Forms.MenuItem showTableTypeCodeMenuItem;
        private System.Windows.Forms.ContextMenu dataTypeMenu;
        private System.Windows.Forms.MenuItem showDataTypeCodeMenuItem;
        private System.Windows.Forms.Panel panel3;
        private DataGridViewDoubleBuffered grid;
        private TextEditorControl txtCode;
        private System.Windows.Forms.ContextMenu databaseMenu;
        private System.Windows.Forms.MenuItem generateRestorePageScriptMenuItem;
    }
}

