using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public interface ISqlType
	{
		bool IsVariableLength { get; }
		short? FixedLength { get; }
		object GetValue(byte[] value);
        object GetDefaultValue(SysDefaultConstraint columnConstraint);
    }
}