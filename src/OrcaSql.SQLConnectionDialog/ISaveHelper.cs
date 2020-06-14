using System;

namespace OrcaSql.SQLConnectionDialog
{
	/// <summary>
	/// The ISaveHelper interface is an abstraction of the 'save' functionality which allows for providing a simple 
	/// default mechanism for persisting ConnectionString data to the app.config file, or allowing the user to 
	/// replace this functionality without recompiling the assembly
	/// </summary>
	public interface ISaveHelper
	{
		/// <summary>
		/// A SaveHelper class requires an event to initialize persisting data
		/// </summary>
		/// <param name="connectionString">The ConnectionString to persist</param>
		void Save(String connectionString);

		/// <summary>
		/// Allows read or assignment of the save method. If set to null, the default delegate is assigned.
		/// </summary>
		SaveMethodPointer SaveMethod { get; set; }
	}
}
