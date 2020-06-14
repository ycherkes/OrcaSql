using System;
using System.Windows.Forms;
using OrcaSql.Core.Engine;
using SQLConnectionDialog;

namespace OrcaSql.SQLConnectionDialog
{
	public class SqlConnectionDialog
	{
		private readonly frmSqlConnectionDialog _frmSqlConnectionDialog;
		private ISaveHelper _saveHelper;
		private readonly ISaveHelper _defaultSaveHelper = new SaveHelperDefault();

        #region " Ctor "

		/// <summary>
		/// Default constructor will handle the ISaveHelper as if it were null in the overloaded constructor. 
		/// The result is a default ISaveHelper instance is assigned. The SaveHelper property can be assigned later, 
		/// or the SaveMethod property (a SaveMethodPointer delegate) can be assigned to the default SaveHelper instance.
		/// </summary>
		public SqlConnectionDialog()
			: this((ISaveHelper)null) { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="saveHelper">If the ISaveHelper is null, the result is a default ISaveHelper instance is assigned. The SaveHelper property can be assigned later, 
		/// or the SaveMethod property (a SaveMethodPointer delegate) can be assigned to the default SaveHelper instance.</param>
		public SqlConnectionDialog(ISaveHelper saveHelper)
		{
			this.SaveHelper = saveHelper;
			//if (saveHelper != null) _saveHelper = saveHelper;
			//else _saveHelper = _defaultSaveHelper;
			// Can simplify code with one line: this.SaveHelper = saveHelper; // as this does the check for null and set default
			_frmSqlConnectionDialog = new frmSqlConnectionDialog();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="saveHelper">If the ISaveHelper is null, the result is a default ISaveHelper instance is assigned. The SaveHelper property can be assigned later, 
		/// or the SaveMethod property (a SaveMethodPointer delegate) can be assigned to the default SaveHelper instance.</param>
		/// <param name="connectionString">Specify the current connection string the ConnectionUI will help modify</param>
		public SqlConnectionDialog(ISaveHelper saveHelper, String connectionString)
			: this(saveHelper)
		{
			_frmSqlConnectionDialog.ConnectionString = connectionString;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="saveHelper">If the ISaveHelper is null, the result is a default ISaveHelper instance is assigned. The SaveHelper property can be assigned later, 
		/// or the SaveMethod property (a SaveMethodPointer delegate) can be assigned to the default SaveHelper instance.</param>
		/// <param name="connectionString">Specify the current connection string the ConnectionUI will help modify</param>
		/// <param name="title">Specify the text title for the display form</param>
		public SqlConnectionDialog(ISaveHelper saveHelper, string connectionString, string title)
			: this(saveHelper, connectionString)
		{
			_frmSqlConnectionDialog.Text = title;
		}

		/// <summary>
		/// If the ISaveHelper is null, the result is a default ISaveHelper instance is assigned. The SaveHelper property can be assigned later, 
		/// or the SaveMethod property (a SaveMethodPointer delegate) can be assigned to the default SaveHelper instance.
		/// </summary>
		/// <param name="connectionString">Specify the current connection string the ConnectionUI will help modify</param>
		public SqlConnectionDialog(string connectionString)
			: this((ISaveHelper)null, connectionString)
		{ }

		/// <summary>
		/// If the ISaveHelper is null, the result is a default ISaveHelper instance is assigned. The SaveHelper property can be assigned later, 
		/// or the SaveMethod property (a SaveMethodPointer delegate) can be assigned to the default SaveHelper instance.
		/// </summary>
		/// <param name="connectionString">Specify the current connection string the ConnectionUI will help modify</param>
		/// <param name="title">Specify the text title for the display form</param>
		public SqlConnectionDialog(string connectionString, string title)
			: this(null, connectionString, title)
		{ }

		#endregion

		#region " Properties "

		/// <summary>
		/// An instance of an ISaveHelper which contains the functionality to persist the ConnectionString or 
		/// holds the delegate which points to the method (in any location) which will persist the data.
		/// </summary>
		public ISaveHelper SaveHelper
		{
			get => _saveHelper;
            set => _saveHelper = value ?? _defaultSaveHelper;
        }

		/// <summary>
		/// Text which is displayed on the UI dialog form
		/// </summary>
		public string Title
		{
			get => _frmSqlConnectionDialog.Text;
            set => _frmSqlConnectionDialog.Text = value;
        }

		/// <summary>
		/// Sets or gets the value of the ConnectionString for editing
		/// </summary>
		public string ConnectionString
		{
			get => _frmSqlConnectionDialog.ConnectionString;
            set => _frmSqlConnectionDialog.ConnectionString = value;
        }

		#endregion

		#region " Methods "

		/// <summary>
		/// Executes the Save() method on the ISaveHelper object
		/// </summary>
		/// <remarks>
		/// Save() is exposed to the public as this will allow the user to manually update (without using the UI)
		/// the app.config file or execute their own proprietary ISaveHelper.Save() method by 
		/// assigning the delegate on the default ISaveHelper object or by creating their own.
		/// </remarks>
		public void Save()
        {
            _saveHelper?.Save(_frmSqlConnectionDialog.ConnectionString);
        }

		/// <summary>
		/// Displays the SQL Connection Form in dialog mode and will automatically call the Save() method if 'OK' is clicked 
		/// and will do nothing if 'Cancel' is clicked.
		/// </summary>
		public DialogResult Show()
		{
			var dialogResult = _frmSqlConnectionDialog.ShowDialog();

			if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes) { Save(); }

            return dialogResult;
        }

        public string TableName
        {
            get => _frmSqlConnectionDialog.txtTableName.Text;
            set => _frmSqlConnectionDialog.txtTableName.Text = value;
        }

        public Mode Mode
        {
            get => _frmSqlConnectionDialog.Mode;
            set => _frmSqlConnectionDialog.Mode = value;
        }

        public PagePointer PagePointer
        {
            get => new PagePointer(short.Parse(_frmSqlConnectionDialog.txtFileId.Text), long.Parse(_frmSqlConnectionDialog.txtPageId.Text));
            set
            {
                _frmSqlConnectionDialog.txtFileId.Text = value.FileID.ToString();
                _frmSqlConnectionDialog.txtPageId.Text = value.PageID.ToString();
            }
        }

        #endregion
    }
}