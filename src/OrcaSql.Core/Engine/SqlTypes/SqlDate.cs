using System;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
    public class SqlDate : SqlTypeBase
    {
        public SqlDate(CompressionContext compression)
            : base(compression)
        { }

        public override bool IsVariableLength => false;

        public override short? FixedLength => 3;

        public override object GetValue(byte[] value)
        {
            if (value.Length != 3)
                throw new ArgumentException("Invalid value length: " + value.Length);

            // Magic needed to read a 3 byte integer into .NET's 4 byte representation.
            // Reading backwards due to assumed little endianness.
            var date = value[0] + (value[1] << 8) + (value[2] << 16);

            return DateTime.MinValue.AddDays(date);
        }

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            return DateTime.TryParse(columnConstraint.Definition.Trim('(', ')'), out var parsedResult) ? parsedResult : (object)null;
        }
    }
}