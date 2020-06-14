using System;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public class SqlFloat : SqlTypeBase
	{
		private readonly byte precision;

        public SqlFloat(byte precision, CompressionContext compression)
			: base(compression)
		{
			this.precision = precision;
		}

		public override bool IsVariableLength => CompressionContext.UsesVardecimals;

        public override short? FixedLength => Convert.ToInt16(GetNumberOfRequiredStorageBytes());

        private byte GetNumberOfRequiredStorageBytes()
        {
            return precision <= 24 ? (byte) 4 : (byte) 8;
        }

		public override object GetValue(byte[] value)
		{
			//if(CompressionContext.CompressionLevel == CompressionLevel.None)
			//{
				if (value.Length != FixedLength.Value)
					throw new ArgumentException("Invalid value length: " + value.Length);

                return value.Length == 4 ? BitConverter.ToSingle(value, 0) : BitConverter.ToDouble(value, 0);
            //}
			
		}

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            return float.TryParse(columnConstraint.Definition.Trim('(', ')'), out var parsedResult) ? parsedResult : (object)null;
        }
    }
}