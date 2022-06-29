using OrcaSql.Core.Engine;
using OrcaSql.Core.MetaData.DMVs;
using OrcaSql.Core.MetaData.Enumerations;
using OrcaSql.Core.MetaData.Exceptions;
using OrcaSql.Core.MetaData.TableValuedDictionaries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrcaSql.Core.MetaData
{
    public class DatabaseMetaData
    {
        private readonly Database _db;

        internal DatabaseMetaData(Database db)
        {
            _db = db;
        }

        public DataRow GetEmptyIndexRow(string tableName, string indexName)
        {
            // Get table
            var table = _db.Dmvs.Objects
                .SingleOrDefault(x => x.Name == tableName && (x.Type == ObjectType.USER_TABLE || x.Type == ObjectType.SYSTEM_TABLE));

            if (table == null)
                throw new UnknownTableException(tableName);

            // Get index
            var index = _db.Dmvs.Indexes
                .SingleOrDefault(i => i.ObjectID == table.ObjectID && i.Name == indexName);

            if (index == null)
                throw new UnknownIndexException(tableName, indexName);

            if (index.IndexID == 0)
                throw new ArgumentException("Can't create DataRow for heaps.");

            // Determine if table is clustered or a heap. If we're not scanning the clustered index itself, see if
            // table has a clustered index. If not, it's a heap.
            var isHeap = !(index.IndexID == 1 || _db.Dmvs.Indexes.Any(i => i.ObjectID == table.ObjectID && i.IndexID == 1));

            // Get index columns
            var idxColumns = _db.Dmvs.IndexColumns
                .Join(_db.Dmvs.Columns, ic => new { ic.ColumnID, ic.ObjectID }, c => new { c.ColumnID, c.ObjectID }, (ic, c) => new { ic.ObjectID, ic.IndexID, ic.KeyOrdinal, c.IsNullable, ic.IsIncludedColumn, c.SystemTypeID, c.Name, c.MaxLength })
                .Where(x => x.ObjectID == table.ObjectID && x.IndexID == index.IndexID)
                .OrderBy(x => x.KeyOrdinal)
                .ToArray();

            // Get first index partition
            var idxFirstPartition = _db.Dmvs.SystemInternalsPartitions
                .Where(p => p.ObjectID == table.ObjectID && p.IndexID == index.IndexID)
                .OrderBy(p => p.PartitionNumber)
                .First();

            if (!idxColumns.Any())
                throw new Exception("No columns found for index '" + indexName + "'");

            // Get rowset columns - these are the ones implicitly included in the index
            var partitionColumns = _db.Dmvs.SystemInternalsPartitionColumns
                .Where(pc => pc.PartitionID == idxFirstPartition.PartitionID && pc.KeyOrdinal > idxColumns.Max(ic => ic.KeyOrdinal))
                .OrderBy(pc => pc.KeyOrdinal)
                .ToArray();

            // Add columns as specified in sysiscols
            var columnsList = new List<DataColumn>();
            foreach (var col in idxColumns)
            {
                var sqlType = _db.Dmvs.Types.Single(x => x.SystemTypeID == col.SystemTypeID);

                // TODO: Handle decimal/other data types that needs more than a length specification

                var dc = new DataColumn(col.Name, sqlType.Name + "(" + col.MaxLength + ")")
                {
                    IsNullable = col.IsNullable,
                    IsIncluded = col.IsIncludedColumn
                };

                columnsList.Add(dc);
            }

            // Add remaining columns as specified in sysrscols
            foreach (var col in partitionColumns)
            {
                var sqlType = _db.Dmvs.Types.Single(x => x.UserTypeID == col.SystemTypeID);

                // The uniquifier for clustered tables needs special treatment. Uniquifier is detected by the system type and
                // the fact that it's stored in the variable length section of the record (LeafOffset < 0).
                if (!isHeap && col.SystemTypeID == (int)SystemType.Int && col.LeafOffset < 0)
                {
                    columnsList.Add(DataColumn.Uniquifier);
                    continue;
                }

                // The RID for heaps needs special treatment. RID is detected by system type (binary(8)) and by
                // being the last column in the record.
                if (isHeap && col.SystemTypeID == (int)SystemType.Binary && col.MaxLength == 8 && col.KeyOrdinal == partitionColumns.Max(pc => pc.KeyOrdinal))
                {
                    columnsList.Add(DataColumn.RID);
                    continue;
                }

                // We don't have the corresponding column name from the clustered key (though it could be queried).
                // Thus we'll just give them an internal name for now.
                var dc = new DataColumn("__rscol_" + col.KeyOrdinal, sqlType.Name + "(" + col.MaxLength + ")")
                {
                    IsNullable = col.IsNullable,
                    IsIncluded = true
                };

                // Clustered index columns that are not explicitly included in the nonclustered index will be
                // implicitly included.

                columnsList.Add(dc);
            }

            return new DataRow(columnsList);
        }

        /// <summary>
		/// Looks up the partition and determines if it's using vardecimal columns.
		/// 
		/// Decimals are special - they may in fact be vardecimals, though type wise there is no distinction in SQL Server.
		/// Determining whether a given decimal column is vardecimal is done through the built-in OBJECTPROPERTY function,
		/// however, we do not have access to that. There's a workaround though. Decimals are _always_ fixed length...
		/// _Except_ when they're vardecimal. If we look in sys.system_internals_partition_columns, all fixed length
		/// columns have a positive leaf_offset, where as variable length columns have a negative leaf_offset value - 
		/// including vardecimal. As vardecimals are either all decimal or all vardecimal, we just need to determine
		/// if _any_ decimal column, for this table, has a negative leaf_offset value.
		/// </summary>
		internal bool PartitionHasVardecimalColumns(long partitionID)
        {
            // Get the vardecimal type id
            byte vardecimalTypeID = _db.Dmvs.Types
                .Where(t => t.Name == "decimal")
                .Select(t => t.SystemTypeID)
                .Single();

            // Get all partition columns of type decimal with a negative leaf_offset
            int negativeLeafOffsetDecimalColumns = _db.Dmvs.SystemInternalsPartitionColumns
                .Join(_db.Dmvs.SystemInternalsPartitions, pc => pc.PartitionID, p => p.PartitionID, (pc, p) => new { Column = pc, Partition = p })
                .Count(x => x.Partition.PartitionID == partitionID
                            && x.Column.SystemTypeID == vardecimalTypeID
                            && x.Column.LeafOffset < 0);

            // If any decimal columns are stored as variable length, we're using vardecimals
            return negativeLeafOffsetDecimalColumns > 0;
        }

        public DataRow GetEmptyDataRow(string tableName, int? schemaId = null)
        {
            // Get table
            var table = _db.Dmvs.Objects
                .SingleOrDefault(x => x.Name == tableName && (x.Type == ObjectType.USER_TABLE || x.Type == ObjectType.SYSTEM_TABLE)
                                                          && (schemaId == null || x.SchemaID == schemaId));

            if (table == null)
                throw new UnknownTableException(tableName);

            // Get index
            var clusteredIndex = _db.Dmvs.Indexes
                .SingleOrDefault(i => i.ObjectID == table.ObjectID && i.IndexID == 1);

            // Get columns
            var syscols = _db.Dmvs.Columns
                .Where(x => x.ObjectID == table.ObjectID);

            // Create table and add columns
            var columnsList = new List<DataColumn>();

            // If it's a non unique clustered index, add uniquifier column
            if (clusteredIndex != null && !clusteredIndex.IsUnique)
                columnsList.Add(DataColumn.Uniquifier);


            foreach (var col in syscols)
            {
                var typeName = GetColumnTypeName(_db.Dmvs, col);

                var dc = new DataColumn(col.Name, typeName)
                {
                    IsNullable = col.IsNullable,
                    IsSparse = col.IsSparse,
                    ColumnID = col.ColumnID,
                    Encoding = Collations.GetEncodingForColumn(col)
                };

                columnsList.Add(dc);
            }

            return new DataRow(columnsList, table.ObjectID);
        }


        public static string GetColumnTypeName(DmvGenerator dmvGenerator, Column col)
        {
            var columnType = "{0}";

            if (col.IsComputed)
                columnType = "Computed, {0}";

            return GetTypeName(dmvGenerator, col, columnType);
        }

        public static string GetTypeName(DmvGenerator dmvGenerator, IDataType dataType, bool useBaseTypeOnly = false)
        {
            return GetTypeName(dmvGenerator, dataType, "{0}", useBaseTypeOnly);
        }

        private static string GetTypeName(DmvGenerator dmvGenerator, IDataType dataType, string columnFormat, bool useBaseTypeOnly = false)
        {
            var sqlType = dmvGenerator.Types.Single(x => x.SystemTypeID == dataType.SystemTypeID && x.UserTypeID == dataType.UserTypeID);

            var baseType = dmvGenerator.Types.SingleOrDefault(x => x.SystemTypeID == dataType.SystemTypeID && x.UserTypeID == dataType.SystemTypeID) ?? sqlType;

            if (useBaseTypeOnly) sqlType = baseType;

            if (sqlType.IsUserDefined)
                columnFormat = sqlType.Name + "(" + columnFormat + "), " + GetNullableString(dataType);
            else
            {
                columnFormat = string.Format(columnFormat, "{0}, " + GetNullableString(dataType));
            }

            switch ((SystemType)sqlType.SystemTypeID)
            {
                case SystemType.Decimal:
                case SystemType.Numeric:
                    return string.Format(columnFormat, baseType.Name + "(" + dataType.Precision + ", " + dataType.Scale + ")");
                case SystemType.Float:
                    return string.Format(columnFormat, baseType.Name + "(" + dataType.Precision + ")");
                case SystemType.Datetime2:
                case SystemType.DatetimeOffset:
                case SystemType.Time:
                    return string.Format(columnFormat, baseType.Name + "(" + dataType.Scale + ")");
                case SystemType.Datetime:
                case SystemType.Bigint:
                case SystemType.Bit:
                case SystemType.Geography:
                case SystemType.Int:
                case SystemType.Date:
                case SystemType.Money:
                case SystemType.Real:
                case SystemType.Smallint:
                case SystemType.Smallmoney:
                case SystemType.Smalldatetime:
                case SystemType.Uniqueidentifier:
                case SystemType.Xml:
                    return string.Format(columnFormat, baseType.Name);
                default:
                    return string.Format(columnFormat, baseType.Name + "(" + (dataType.MaxLength == -1 ? "max" : (new[] { SystemType.Nchar, SystemType.Nvarchar }.Cast<byte>().Contains(baseType.SystemTypeID) ? dataType.MaxLength / 2 : dataType.MaxLength).ToString()) + ")");
            }
        }

        private static string GetNullableString(IDataType col)
        {
            return col.IsNullable ? "null" : "not null";
        }
    }
}