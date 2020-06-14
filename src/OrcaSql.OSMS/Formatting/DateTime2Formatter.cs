using System;
using System.Windows.Forms;

namespace OrcaSql.OSMS.Formatting
{
    public class DateTime2Formatter : ColumnFormatter
    {
        public override bool UseCellFormat => true;
        private static readonly string[] _katmaiDateTime2FormatByScale = 
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.f",
            "yyyy-MM-dd HH:mm:ss.ff",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss.ffff",
            "yyyy-MM-dd HH:mm:ss.fffff",
            "yyyy-MM-dd HH:mm:ss.ffffff",
            "yyyy-MM-dd HH:mm:ss.fffffff"
        };

        public override Action<DataGridViewCellStyle> ApplyFormat(byte precision, byte scale)
        {
            return x => x.Format = _katmaiDateTime2FormatByScale[scale];
        }

        public override bool UseStringValue => false;
    }
}