using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public class SqlImage : SqlTypeBase
	{
		public SqlImage(CompressionContext compression)
			: base(compression)
		{ }

		public override bool IsVariableLength
		{
			get { return true; }
		}

		public override short? FixedLength
		{
			get { return null; }
		}

		public override object GetValue(byte[] value)
		{
			return value;
		}

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            return null;
        }
    }
}