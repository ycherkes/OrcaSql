using System;

namespace OrcaSql.OSMS.Formatting
{
    public class GuidFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => false;

        public override string GetStringValue(object value)
        {
            if (value == DBNull.Value) return null;
            var guidString = (Guid)value;

            return guidString.ToString().ToUpperInvariant();
        }

        public override bool UseStringValue => true;
    }
}