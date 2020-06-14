using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
    public class SqlComputed : SqlTypeBase
    {
        public SqlComputed(CompressionContext compression)
            : base(compression)
        { }

        public override bool IsVariableLength => false;

        public override short? FixedLength => 0;

        public override object GetValue(byte[] value)
        {
            return "Computed";
        }

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            return null;
        }
    }
}