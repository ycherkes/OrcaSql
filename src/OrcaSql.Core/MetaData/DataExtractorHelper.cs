using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData.DMVs;
using System.Collections.Generic;
using System.Linq;

namespace OrcaSql.Core.MetaData
{
    public class DataExtractorHelper : Row
    {
        private readonly Row _dataRow;
        private readonly Dictionary<int, SysDefaultConstraint> _defaultConstraints;
        private readonly Dictionary<int, object> _cachedDefaultValues;

        private DataExtractorHelper(IReadOnlyCollection<DataColumn> sourceColumns, DmvGenerator dmvGenerator, SystemInternalsPartitionColumn[] partitionColumns, SysDefaultConstraint[] defaultConstraints)
        {
            _cachedDefaultValues = new Dictionary<int, object>();
            var newOrderedColumns = new List<DataColumn>();
            var columns = sourceColumns.ToList();

            if (partitionColumns != null)
            {
                columns = (from pc in partitionColumns
                           join col in sourceColumns on pc.PartitionColumnID equals col.ColumnID into cols
                           from cc in cols.DefaultIfEmpty()
                           select cc ?? new DataColumn($"<<DROPPED{pc.PartitionColumnID}>>", DatabaseMetaData.GetTypeName(dmvGenerator, pc), pc.IsNullable)).ToList();

                _defaultConstraints = defaultConstraints?.ToDictionary(x => x.ParentColumnId) ?? new Dictionary<int, SysDefaultConstraint>();
            }

            newOrderedColumns.AddRange(columns);

            Schema = new Schema(newOrderedColumns);

            BitColumnsCount = newOrderedColumns.Count(x => x.UnderlyingType == ColumnType.Bit);

            // Sparse columns don't have an entry in the null bitmap, thus we should only increment it if the current
            // column was not a sparse column.
            NonSparseIndexes = new Dictionary<string, int>();
            var correction = 0;
            var index = 1;
            foreach (var dataRowColumn in newOrderedColumns)
            {
                if (dataRowColumn.UnderlyingType == ColumnType.Computed || dataRowColumn.IsSparse)
                {
                    NonSparseIndexes.Add(dataRowColumn.Name, -1);
                    ++correction;
                }
                else
                {
                    NonSparseIndexes.Add(dataRowColumn.Name, index - 1 - correction);
                }

                ++index;
            }
        }

        public Dictionary<string, int> NonSparseIndexes { get; }

        public DataExtractorHelper(Row dataRow, DmvGenerator dmvGenerator, IndexColumn[] clusteredIndexColumns,
            SystemInternalsPartitionColumn[] partitionColumns, SysDefaultConstraint[] defaultConstraints) : this(dataRow.Columns, dmvGenerator, partitionColumns, defaultConstraints)
        {
            this._dataRow = dataRow;
        }

        public DataExtractorHelper(Row dataRow) : this(dataRow.Columns, null, null, null)
        {
            this._dataRow = dataRow;
        }

        public override Row NewRow()
        {
            return _dataRow.NewRow();
        }

        public int BitColumnsCount { get; set; }

        public bool IsDroppedColumn(DataColumn col)
        {
            return col.Name.StartsWith("<<DROPPED");
        }

        public object GetDefaultValue(DataColumn col, ISqlType sqlType)
        {
            if (col?.ColumnID == null)
                return null;

            if (_cachedDefaultValues.TryGetValue(col.ColumnID.Value, out var defaultValue))
                return defaultValue;

            if (!_defaultConstraints.TryGetValue(col.ColumnID.Value, out var defaultConstraint))
                return null;

            defaultValue = sqlType.GetDefaultValue(defaultConstraint);

            _cachedDefaultValues[col.ColumnID.Value] = defaultValue;

            return defaultValue;
        }
    }
}