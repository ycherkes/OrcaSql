using System;
using System.Configuration;
using System.Windows.Forms;

namespace OrcaSql.SQLConnectionDialog
{
	/// <summary>
	/// The SaveHelperDefault class implements ISaveHelper interface which defines the 'save' functionality and provides a simple 
	/// default mechanism for persisting ConnectionString data to the app.config file. This may be overwritten by assigning the SaveMethod
	/// delegate a new value which will cause the Save() method to execute new user code, OR the user can provide an entirely new 
	/// ISaveHelper class.
	/// </summary>
	public class SaveHelperDefault : ISaveHelper
	{
		#region " Ctor "

		/// <summary>
		/// 
		/// </summary>
		public SaveHelperDefault()
			: this((SaveMethodPointer)null)
		{
			// Default to save the ConnectionString to the AppConfig file
		}

		public SaveHelperDefault(String connectionName)
			: this((SaveMethodPointer)null)
		{
			ConnectionName = connectionName;
		}

		public SaveHelperDefault(SaveMethodPointer saveMethod)
			: this(saveMethod, "")
		{ }

		public SaveHelperDefault(SaveMethodPointer saveMethod, String connectionName)
		{
			if (saveMethod != null) _saveMethod = saveMethod;
			else _saveMethod = _DefaultSave;
		}

		#endregion

		#region " Properties "

		/// <summary>
		/// Allows read or assignment of the save method. If set to null, the default delegate is assigned.
		/// </summary>
		public SaveMethodPointer SaveMethod
		{
			get => _saveMethod;
            set => _saveMethod = value ?? _DefaultSave;
        }
		private SaveMethodPointer _saveMethod;

		/// <summary>
		/// ConnectionName is the name of the 'connectionString' in the App.config
		/// </summary>
		public string ConnectionName { get; set; } = "SqlConnString";

        #endregion

		#region " Methods "

		/// <summary>
		/// Save method will execute the delegate specified for persisting the ConnectionString.
		/// </summary>
		/// <param name="connectionString"></param>
		public void Save(string connectionString)
		{
			if (_saveMethod != null) _saveMethod.Invoke(connectionString);
			else throw new Exception("Pointer/Delegate to the SaveMethod cannot be null. Save() failed.");
		}

        /// <summary>
        /// Save changes to the App.config
        /// </summary>
        /// <remarks>
        /// There is no inbuilt way to change application setting values in the config file.
        /// So that needs to be done manually by calling config section object.
        /// </remarks>
        /// <param name="connectionString">The ConnectionString to persist</param>
        private void _DefaultSave(string connectionString)
		{
			try
			{
				string connectionFullName =
                    $"{System.Reflection.Assembly.GetEntryAssembly().EntryPoint.ReflectedType.Namespace}.Properties.Settings.{ConnectionName}";
				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var section = (ConnectionStringsSection)config.GetSection("connectionStrings");
				var setting = section.ConnectionStrings[connectionFullName];

				//	Ensure connection setting is defined (Note: A default value must be set to save the connection setting!)
				//		If IsNothing(Setting) Then Throw New Exception("There is no connection with this name defined in the config file.")

				//	Set value and save it to the config file
				//	This differs from Jakob Lithner. Runtime Connection Wizard
				//	We only want to save the modified portion of the config file 
				setting.ConnectionString = connectionString;
				config.Save(ConfigurationSaveMode.Minimal, true);
				ConfigurationManager.RefreshSection("connectionStrings");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		#endregion

	}
}
