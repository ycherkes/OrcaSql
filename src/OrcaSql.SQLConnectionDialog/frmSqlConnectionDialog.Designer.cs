namespace SQLConnectionDialog
{
	partial class frmSqlConnectionDialog
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SqlConnUIControl = new Microsoft.Data.ConnectionUI.SqlConnectionUIControl();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.tableNameGroupBox = new System.Windows.Forms.GroupBox();
            this.txtTableName = new System.Windows.Forms.TextBox();
            this.pagePointerGroupBox = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPageId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFileId = new System.Windows.Forms.TextBox();
            this.tableNameGroupBox.SuspendLayout();
            this.pagePointerGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(5, 436);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(67, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(81, 436);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(67, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SqlConnUIControl
            // 
            this.SqlConnUIControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.SqlConnUIControl.Location = new System.Drawing.Point(5, 5);
            this.SqlConnUIControl.Margin = new System.Windows.Forms.Padding(0);
            this.SqlConnUIControl.MinimumSize = new System.Drawing.Size(374, 360);
            this.SqlConnUIControl.Name = "SqlConnUIControl";
            this.SqlConnUIControl.Size = new System.Drawing.Size(374, 360);
            this.SqlConnUIControl.TabIndex = 0;
            // 
            // btnAdvanced
            // 
            this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdvanced.AutoSize = true;
            this.btnAdvanced.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAdvanced.Location = new System.Drawing.Point(309, 435);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(66, 23);
            this.btnAdvanced.TabIndex = 1;
            this.btnAdvanced.Text = "Advanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // btnTest
            // 
            this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTest.AutoSize = true;
            this.btnTest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnTest.Location = new System.Drawing.Point(208, 435);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(95, 23);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = "Test Connection";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // tableNameGroupBox
            // 
            this.tableNameGroupBox.Controls.Add(this.txtTableName);
            this.tableNameGroupBox.Location = new System.Drawing.Point(5, 366);
            this.tableNameGroupBox.Name = "tableNameGroupBox";
            this.tableNameGroupBox.Size = new System.Drawing.Size(374, 53);
            this.tableNameGroupBox.TabIndex = 3;
            this.tableNameGroupBox.TabStop = false;
            this.tableNameGroupBox.Text = "Table name:";
            // 
            // txtTableName
            // 
            this.txtTableName.Location = new System.Drawing.Point(6, 19);
            this.txtTableName.Name = "txtTableName";
            this.txtTableName.Size = new System.Drawing.Size(357, 20);
            this.txtTableName.TabIndex = 0;
            // 
            // pagePointerGroupBox
            // 
            this.pagePointerGroupBox.Controls.Add(this.label2);
            this.pagePointerGroupBox.Controls.Add(this.txtPageId);
            this.pagePointerGroupBox.Controls.Add(this.label1);
            this.pagePointerGroupBox.Controls.Add(this.txtFileId);
            this.pagePointerGroupBox.Location = new System.Drawing.Point(5, 366);
            this.pagePointerGroupBox.Name = "pagePointerGroupBox";
            this.pagePointerGroupBox.Size = new System.Drawing.Size(386, 53);
            this.pagePointerGroupBox.TabIndex = 4;
            this.pagePointerGroupBox.TabStop = false;
            this.pagePointerGroupBox.Text = "Page Pointer:";
            this.pagePointerGroupBox.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(131, 26);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Page Id:";
            // 
            // txtPageId
            // 
            this.txtPageId.Location = new System.Drawing.Point(180, 25);
            this.txtPageId.Name = "txtPageId";
            this.txtPageId.Size = new System.Drawing.Size(62, 20);
            this.txtPageId.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "File Id:";
            // 
            // txtFileId
            // 
            this.txtFileId.Location = new System.Drawing.Point(66, 25);
            this.txtFileId.Name = "txtFileId";
            this.txtFileId.Size = new System.Drawing.Size(54, 20);
            this.txtFileId.TabIndex = 0;
            // 
            // frmSqlConnectionDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(383, 466);
            this.Controls.Add(this.pagePointerGroupBox);
            this.Controls.Add(this.tableNameGroupBox);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAdvanced);
            this.Controls.Add(this.SqlConnUIControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(399, 505);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(399, 505);
            this.Name = "frmSqlConnectionDialog";
            this.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQL Connection Editor";
            this.Load += new System.EventHandler(this.frmSqlConnectionDialog_Load);
            this.tableNameGroupBox.ResumeLayout(false);
            this.tableNameGroupBox.PerformLayout();
            this.pagePointerGroupBox.ResumeLayout(false);
            this.pagePointerGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.Button btnOK;
		internal System.Windows.Forms.Button btnCancel;
		internal Microsoft.Data.ConnectionUI.SqlConnectionUIControl SqlConnUIControl;
		internal System.Windows.Forms.Button btnAdvanced;
		internal System.Windows.Forms.Button btnTest;
        internal System.Windows.Forms.TextBox txtTableName;
        private System.Windows.Forms.GroupBox tableNameGroupBox;
        private System.Windows.Forms.GroupBox pagePointerGroupBox;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox txtPageId;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox txtFileId;
    }
}

