using System;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OrcaSql.OSMS.Formatting
{
    public class XmlFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => true;

        public override Action<DataGridViewCellStyle> ApplyFormat(byte precision, byte scale)
        {
            return x => x.ForeColor = Color.FromArgb(13, 107, 207);
        }

        public override string GetStringValue(object value)
        {
            if (value == DBNull.Value) return null;
            var array = (byte[])value;

            if (array.Length <= 0) return string.Empty;

            using (var ms = new MemoryStream(array))
            {
                var sqlXml = new SqlXml(ms);
                return sqlXml.Value;
            }
        }

        public override bool UseStringValue => true;
    }
}