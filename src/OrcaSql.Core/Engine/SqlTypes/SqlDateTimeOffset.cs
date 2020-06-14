using System;
using System.Linq;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
    public class SqlDateTimeOffset : SqlTypeBase
    {
        private readonly byte length;
        private readonly SqlDateTime2 dateTime2;
        private readonly byte scale;

        public SqlDateTimeOffset(byte scale, CompressionContext compression)
            : base(compression)
        {
            this.scale = scale;
            dateTime2 = new SqlDateTime2(scale, compression);
            length = (byte)(dateTime2.FixedLength.Value + 2);
        }

        public override bool IsVariableLength => false;

        public override short? FixedLength => length;

        public override object GetValue(byte[] value)
        {
            if (value.Length != length)
                throw new ArgumentException("Invalid value length: " + value.Length);

            var dateTime2Value = (DateTime)dateTime2.GetValue(value.Take(length - 2).ToArray());
            var offset = value[length - 2] + (value[length - 1] << 8);

            var offsetTimeSpan = new TimeSpan(0, offset, 0);
            return new DateTimeOffset(dateTime2Value.Ticks + offsetTimeSpan.Ticks, offsetTimeSpan);
        }

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            return DateTimeOffset.TryParse(columnConstraint.Definition.Trim('(', ')'), out var parsedResult) ? parsedResult : (object)null;
        }
    }
}