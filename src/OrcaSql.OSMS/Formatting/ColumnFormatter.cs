using System;
using System.Windows.Forms;

namespace OrcaSql.OSMS.Formatting
{
    public abstract class ColumnFormatter
    {
        public abstract bool UseCellFormat { get;}
        public virtual Action<DataGridViewCellStyle> ApplyFormat(byte precision, byte scale) => null;
        public abstract bool UseStringValue { get; }
        public virtual string GetStringValue(object value) => null;
    }
}