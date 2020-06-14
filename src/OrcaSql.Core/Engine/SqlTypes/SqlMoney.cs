using System;
using OrcaSql.Core.MetaData.DMVs;
using OrcaSql.Framework;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public class SqlMoney : SqlTypeBase
	{
		public SqlMoney(CompressionContext compression)
			: base(compression)
		{ }

		public override bool IsVariableLength
		{
			get { return false; }
		}

		public override short? FixedLength
		{
			get { return 8; }
		}

		public override object GetValue(byte[] value)
		{
			if (CompressionContext.CompressionLevel != CompressionLevel.None)
			{
				if (value.Length > 8)
					throw new ArgumentException("Invalid value length: " + value.Length);

				return SqlBitConverter.ToInt64FromBigEndian(value, 0, Offset.MinValue) / 10000m;
			}
			else
			{
				if (value.Length != FixedLength.Value)
					throw new ArgumentException("Invalid value length: " + value.Length);

				// Fixed decimal point for money
				return BitConverter.ToInt64(value, 0) / 10000m;
			}
		}

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            return long.TryParse(columnConstraint.Definition.Trim('(', ')'), out var parsedResult) ? parsedResult : (object)null;
        }
    }
}