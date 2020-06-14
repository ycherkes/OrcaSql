using System;
using System.Windows.Forms;

namespace OrcaSql.OSMS.Formatting
{
    public class DateTimeOffsetFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => true;
        private static readonly string[] _katmaiDateTimeOffsetFormatByScale = {
            "yyyy-MM-dd HH:mm:ss zzz",
            "yyyy-MM-dd HH:mm:ss.f zzz",
            "yyyy-MM-dd HH:mm:ss.ff zzz",
            "yyyy-MM-dd HH:mm:ss.fff zzz",
            "yyyy-MM-dd HH:mm:ss.ffff zzz",
            "yyyy-MM-dd HH:mm:ss.fffff zzz",
            "yyyy-MM-dd HH:mm:ss.ffffff zzz",
            "yyyy-MM-dd HH:mm:ss.fffffff zzz"
        };

        public override Action<DataGridViewCellStyle> ApplyFormat(byte precision, byte scale)
        {
            return x => x.Format = _katmaiDateTimeOffsetFormatByScale[scale];
        }

        public override bool UseStringValue => false;
    }
}