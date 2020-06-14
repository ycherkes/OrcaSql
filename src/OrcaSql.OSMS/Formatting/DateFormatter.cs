using System;
using System.Windows.Forms;

namespace OrcaSql.OSMS.Formatting
{
    public class DateFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => true;

        public override Action<DataGridViewCellStyle> ApplyFormat(byte precision, byte scale)
        {
            return x => x.Format = "yyyy-MM-dd";
        }

        public override bool UseStringValue => false;
    }
}