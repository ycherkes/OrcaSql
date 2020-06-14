using System.Collections.Generic;
using OrcaSql.Core.MetaData;

namespace OrcaSql.OSMS.Formatting
{
    public static class ColumnFormatters
    {
        private static readonly SortedDictionary<ColumnInfo, ColumnFormatter> Formatters;

        static ColumnFormatters()
        {
            var comparer = new FormatterComparer();
            var formatters = new Dictionary<ColumnInfo, ColumnFormatter>
            {
                {
                    new ColumnInfo
                    {
                        ColumnNames = {"FirstPage", "RootPage", "FirstIamPage"},
                        TableName =  "sys.system_internals_allocation_units",
                        UnderlyingType = ColumnType.Binary,
                        ColumnType = ColumnType.Binary
                    } ,
                    new PagePointerFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnNames = {"pgfirst", "pgroot", "pgfirstiam"},
                        TableName =  "sys.sysallocunits",
                        UnderlyingType = ColumnType.Binary,
                        ColumnType = ColumnType.Binary
                    } ,
                    new PagePointerFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.Binary,
                        UnderlyingType = ColumnType.Binary
                    } ,
                    new ByteArrayFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.VarBinary,
                        UnderlyingType = ColumnType.VarBinary
                    } ,
                    new ByteArrayFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.Timestamp,
                        UnderlyingType = ColumnType.Binary
                    } ,
                    new ByteArrayFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.HierarchyId,
                        UnderlyingType = ColumnType.VarBinary
                    } ,
                    new ByteArrayFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.Geography,
                        UnderlyingType = ColumnType.VarBinary
                    } ,
                    new ByteArrayFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.Geometry,
                        UnderlyingType = ColumnType.VarBinary
                    } ,
                    new ByteArrayFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.Xml,
                        UnderlyingType = ColumnType.VarBinary
                    } ,
                    new XmlFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.DateTime,
                        UnderlyingType = ColumnType.DateTime
                    } ,
                    new DateTimeFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.UniqueIdentifier,
                        UnderlyingType = ColumnType.UniqueIdentifier
                    } ,
                    new GuidFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.Computed,
                        UnderlyingType = ColumnType.Computed
                    } ,
                    new ComputedFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.DateTime2,
                        UnderlyingType = ColumnType.DateTime2
                    } ,
                    new DateTime2Formatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.DateTimeOffset,
                        UnderlyingType = ColumnType.DateTimeOffset
                    } ,
                    new DateTimeOffsetFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.Time,
                        UnderlyingType = ColumnType.Time
                    } ,
                    new TimeFormatter()
                },
                {
                    new ColumnInfo
                    {
                        ColumnType = ColumnType.Date,
                        UnderlyingType = ColumnType.Date
                    } ,
                    new DateFormatter()
                }

            };

            foreach (var formattersKey in formatters.Keys)
            {
                formattersKey.Initialize();
            }

            Formatters = new SortedDictionary<ColumnInfo, ColumnFormatter>(formatters, comparer);

            comparer.Mode = ComparerMode.ExtractData;
        }

        public static ColumnFormatter GetFormatter(string tableName, DataColumn column)
        {
            return Formatters.TryGetValue(
                new ColumnInfo
                {
                    ColumnNames = {column.Name},
                    UnderlyingType = column.UnderlyingType,
                    ColumnType = column.Type,
                    TableName = tableName
                },
                out var formatter) 
                ? formatter 
                : null;
        }
    }
}