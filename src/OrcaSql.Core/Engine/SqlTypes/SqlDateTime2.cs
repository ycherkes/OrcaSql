using System;
using System.Linq;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
    public class SqlDateTime2 : SqlTypeBase
    {
        private readonly byte length;
        private readonly SqlTime time;
        private readonly byte scale;
        private readonly SqlDate date;

        public SqlDateTime2(byte scale, CompressionContext compression)
            : base(compression)
        {
            this.scale = scale;
            time = new SqlTime(this.scale, compression);
            date = new SqlDate(compression);
            length = (byte)(time.FixedLength.Value + 3);
        }

        public override bool IsVariableLength => false;

        public override short? FixedLength => length;

        public override object GetValue(byte[] value)
        {
            if (value.Length != length)
                throw new ArgumentException("Invalid value length: " + value.Length);

            var timeValue = (TimeSpan)time.GetValue(value.Take(length - 3).ToArray());
            var dateValue = (DateTime)date.GetValue(value.Skip(length - 3).ToArray());

            return new DateTime(dateValue.Subtract(DateTime.MinValue).Ticks + timeValue.Ticks);
        }

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            return DateTime.TryParse(columnConstraint.Definition.Trim('(', ')'), out var parsedResult) ? parsedResult : (object)null;
        }
    }
}