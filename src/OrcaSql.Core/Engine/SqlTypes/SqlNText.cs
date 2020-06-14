using System.Text;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public class SqlNText : SqlTypeBase
	{
		public SqlNText(CompressionContext compression)
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
            return Encoding.Unicode.GetString(value);
		}

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            return null;
        }
    }
}