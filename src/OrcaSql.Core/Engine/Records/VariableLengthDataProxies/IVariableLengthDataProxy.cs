using System.Collections.Generic;

namespace OrcaSql.Core.Engine.Records.VariableLengthDataProxies
{
	public interface IVariableLengthDataProxy
	{
		IEnumerable<byte> GetBytes();
	}
}