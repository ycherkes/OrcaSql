using System;

namespace OrcaSql.Core.MetaData.Exceptions
{
	public abstract class OrcaSqlException : Exception
	{
		internal OrcaSqlException(string message) : base(message)
		{ }
	}
}