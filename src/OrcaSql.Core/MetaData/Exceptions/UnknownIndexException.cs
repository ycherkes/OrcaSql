namespace OrcaSql.Core.MetaData.Exceptions
{
	public class UnknownIndexException : OrcaSqlException
	{
		public UnknownIndexException(string table, string index)
			: base("Unknown index '" + index + "' on table '" + table + "'")
		{ }
	}
}