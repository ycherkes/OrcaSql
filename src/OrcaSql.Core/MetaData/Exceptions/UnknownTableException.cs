namespace OrcaSql.Core.MetaData.Exceptions
{
	public class UnknownTableException : OrcaSqlException
	{
		public UnknownTableException(string table)
			: base("Unknown table '" + table + "'")
		{ }
	}
}