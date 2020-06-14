using System;
using System.Drawing;
using System.Windows.Forms;

namespace OrcaSql.OSMS.Formatting
{
    public class ComputedFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => true;

        public override Action<DataGridViewCellStyle> ApplyFormat(byte precision, byte scale)
        {
            return x => x.ForeColor = Color.FromArgb(13, 107, 207);
        }

        public override bool UseStringValue => false;
    }
}