using System;

namespace OrcaSql.OSMS.Formatting
{
    public class ByteArrayFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => false;

        public override string GetStringValue(object value)
        {
            if (value == DBNull.Value) return null;
            var array = (byte[])value;

            if (array.Length <= 0) return string.Empty;

            var text = "0x" + BitConverter.ToString(array).Replace("-", "");
            return text.Length > 1000 ? text.Substring(0, 997) + "..." : text;

        }

        public override bool UseStringValue => true;
    }
}