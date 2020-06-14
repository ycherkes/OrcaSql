using System;
using System.Windows.Forms;

namespace OrcaSql.OSMS.Formatting
{
    public class TimeFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => true;
        private static readonly string[] _katmaiTimeFormatByScale = {
            @"hh\:mm\:ss",
            @"hh\:mm\:ss\.f",
            @"hh\:mm\:ss\.ff",
            @"hh\:mm\:ss\.fff",
            @"hh\:mm\:ss\.ffff",
            @"hh\:mm\:ss\.fffff",
            @"hh\:mm\:ss\.ffffff",
            @"hh\:mm\:ss\.fffffff"
        };

        public override Action<DataGridViewCellStyle> ApplyFormat(byte precision, byte scale)
        {
            return x => x.Format = _katmaiTimeFormatByScale[scale];
        }

        public override bool UseStringValue => false;
    }
}