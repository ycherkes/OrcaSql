using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.ConnectionUI;

namespace SQLConnectionDialog
{
    /// <summary>
    /// 
    /// </summary>
    /// <value>Returns DialogResult.OK | DialogResult.Cancel</value>
    public partial class frmSqlConnectionDialog : Form
	{
		private readonly SqlFileConnectionProperties _sfcpSqlConnectionString;
        private Mode _mode;

        public Mode Mode
        {
            get => _mode;
            set
            {
                _mode = value;

                pagePointerGroupBox.Visible = _mode == Mode.PageId;
                tableNameGroupBox.Visible = _mode == Mode.Table;
            }
        }

        public frmSqlConnectionDialog()
		{
			InitializeComponent();

			// Add any initialization after the InitializeComponent() call.
			_sfcpSqlConnectionString = new SqlFileConnectionProperties();
			SqlConnUIControl.Initialize(_sfcpSqlConnectionString);
            var attachDatabaseRadioButton = FindAllChildrenByType<RadioButton>(SqlConnUIControl)
                .Single(x => string.Equals(x.Name, "attachDatabaseRadioButton", StringComparison.InvariantCultureIgnoreCase));
            attachDatabaseRadioButton.Enabled = false;

            Mode = Mode.Table;
        }

        private static IEnumerable<T> FindAllChildrenByType<T>(Control control)
        {
            var controls = control.Controls.Cast<Control>().ToArray();
            return controls
                .OfType<T>()
                .Concat(controls.SelectMany(FindAllChildrenByType<T>));
        }

        private void frmSqlConnectionDialog_Load(object sender, EventArgs e)
		{
			SqlConnUIControl.LoadProperties();
		}



		/// <summary>
		/// Allows the user to change the title of the dialog
		/// </summary>
		//public override String Text
		//{
		//	get { return base.Text; }
		//	set { base.Text = value; }
		//}

		/// <summary>
		/// Pass the original connection string or get the resulting connection string
		/// </summary>
		public string ConnectionString
		{
			get => _sfcpSqlConnectionString.ConnectionStringBuilder.ConnectionString;
            set => _sfcpSqlConnectionString.ConnectionStringBuilder.ConnectionString = value;
            //SqlConnUIControl.LoadProperties();
        }

		private void btnOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void btnAdvanced_Click(object sender, EventArgs e)
		{
			// Set up a form to display the advanced connection properties
            var frm = new Form
            {
                Text = "SQL Connection String Properties",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly
            };

            var pg = new PropertyGrid {SelectedObject = _sfcpSqlConnectionString, Dock = DockStyle.Fill, Parent = frm};

            frm.ShowDialog();
		}

		private void btnTest_Click(object sender, EventArgs e)
		{
			// Test the connection
			using (var conn = new SqlConnection(_sfcpSqlConnectionString.ConnectionStringBuilder.ConnectionString))
			{
				try
				{
					conn.Open();
					MessageBox.Show("Test Connection Succeeded.", "Test Results", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Test Connection Failed: {Environment.NewLine}{ex.Message}", "Test Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				finally
				{
					try { conn.Close(); }
					catch (Exception ex)
					{
						MessageBox.Show($"Exception: {ex.Message}", "Test Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}
    }

    public enum Mode
    {
        Table,
        PageId
    }
}
