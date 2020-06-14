using System;
using System.Collections.Generic;
using System.Linq;

namespace OrcaSql.OSMS.Formatting
{
    public class FormatterComparer : IComparer<ColumnInfo>
    {
        public ComparerMode Mode { get; set; }

        public int Compare(ColumnInfo x, ColumnInfo y)
        {
            var columnUnderLyingTypeComparisonResult = x.UnderlyingType.CompareTo(y.UnderlyingType);

            if (columnUnderLyingTypeComparisonResult != 0) return columnUnderLyingTypeComparisonResult;

            var columnTypeComparisonResult = x.ColumnType.CompareTo(y.ColumnType);

            if (columnTypeComparisonResult != 0) return columnTypeComparisonResult;

            var tableNameComparisonResult =  string.Compare(x.TableName, y.TableName, StringComparison.OrdinalIgnoreCase);

            var columnNameComparisonResult = string.Compare(x.ColumnNamesString, y.ColumnNamesString, StringComparison.OrdinalIgnoreCase);

            if (Mode == ComparerMode.FillDictionary)
                return tableNameComparisonResult != 0 ? x.TableName != null ? tableNameComparisonResult : -tableNameComparisonResult : columnNameComparisonResult;

            if (tableNameComparisonResult != 0 && y.TableName != null)
                return tableNameComparisonResult;
            
            if (columnNameComparisonResult != 0 &&
                !x.ColumnNames.Intersect(y.ColumnNames, StringComparer.OrdinalIgnoreCase).Any() 
                && !string.IsNullOrEmpty(y.ColumnNamesString))
                return columnNameComparisonResult;

            return 0;
        }
    }
}