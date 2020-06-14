using System.Collections.Generic;
using OrcaSql.Core.MetaData;

namespace OrcaSql.OSMS.Formatting
{
    public class ColumnInfo
    {
        public ColumnInfo()
        {
            ColumnNames = new HashSet<string>();
        }

        public void Initialize()
        {
            ColumnNamesString = string.Join(";", ColumnNames);
        }

        public ColumnType UnderlyingType { get; set; }
        public ColumnType ColumnType { get; set; }

        public string TableName { get; set; }

        public HashSet<string> ColumnNames { get; set; }

        public string ColumnNamesString { get; private set; }
    }
}