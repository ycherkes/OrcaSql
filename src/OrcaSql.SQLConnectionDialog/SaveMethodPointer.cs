namespace OrcaSql.SQLConnectionDialog
{
	/// <summary>
	/// The save method is abstracted to be replaced by a delegate, anonymous method, or lambda expression. 
	/// </summary>
	/// <param name="connectionString">The ConnectionString to persist</param>
	public delegate void SaveMethodPointer(string connectionString);
}
