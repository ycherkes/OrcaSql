using OrcaSql.Core.Engine;

namespace OrcaSql.OSMS.Formatting
{
    public class PagePointerFormatter : ColumnFormatter
    {
        public override bool UseCellFormat => false;

        public override string GetStringValue(object value) => new PagePointer((byte[]) value).ToString();

        public override bool UseStringValue => true;
    }
}