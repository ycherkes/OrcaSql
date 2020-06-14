using System;
using System.Windows.Forms;

namespace OrcaSql.OSMS.Formatting
{
    public class DateTimeFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => true;
        public override Action<DataGridViewCellStyle> ApplyFormat(byte precision, byte scale)
        {
            return x => x.Format = "yyyy-MM-dd HH:mm:ss.fff";
        }

        public override bool UseStringValue => false;
    }
}