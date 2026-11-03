using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.Engine.Pages;
using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.Records.Parsers
{
	internal class PrimaryRecordEntityParser : RecordEntityParser
	{
		private readonly PrimaryRecordPage page;
		private readonly CompressionContext compression;
        private static readonly HashSet<RecordType> _recordsToSkip;

        static PrimaryRecordEntityParser()
        {
            // Forwarded records are read through their forwarding stub; reading them again here would return them twice.
            _recordsToSkip = new HashSet<RecordType>(new[]{ RecordType.BlobFragment , RecordType.GhostData, RecordType.Forwarded });
        }

		internal PrimaryRecordEntityParser(PrimaryRecordPage page, CompressionContext compression)
		{
			this.page = page;
			this.compression = compression;
		}

        internal override IEnumerable<Row> GetEntities(DataExtractorHelper schema)
        {
            // Type decoders hold no state between values, so one per column serves the whole page. Bit columns are
            // the exception: they track the bit being read in the record and are still created per record.
            var columns = schema.Columns;
            var sqlTypes = new ISqlType[columns.Count];
            var physicalColumns = new PhysicalColumn[columns.Count];
            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].UnderlyingType != ColumnType.Bit)
                    sqlTypes[i] = SqlTypeFactory.Create(columns[i], null, compression);

                if (schema.Physical != null && schema.Physical.TryGetValue(columns[i], out var physicalColumn))
                    physicalColumns[i] = new PhysicalColumn(physicalColumn, schema.IsDroppedColumn(columns[i]));
            }

            foreach (var record in page.Records)
            {
                // Don't process forwarded blob fragments as they should only be processed from the referenced record
                if (_recordsToSkip.Contains(record.Type) || record.IsGhostForwardedRecord)
                    continue;

                short fixedOffset = 0;
                short variableColumnIndex = 0;
                var dataRow = schema.NewRow();
                var readState = new RecordReadState(schema.BitColumnsCount);
                var bitColumnBytes = new byte[0];

                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    var col = columns[columnIndex];
                    var sqlType = sqlTypes[columnIndex] ?? SqlTypeFactory.Create(col, readState, compression);
                    object columnValue = null;

                    // Sparse columns needs to retrieve their values from the sparse vector, contained in the very last
                    // variable length column in the record.
                    if (col.IsSparse)
                    {
                        // We may encounter records that don't have any sparse vectors, for instance if no sparse columns have values
                        if (record.SparseVector != null)
                        {
                            // Column ID's are stored as ints in general. In the sparse vector though, they're stored as shorts.
                            if (record.SparseVector.ColumnValues.ContainsKey((short)col.ColumnID))
                                columnValue = sqlType.GetValue(record.SparseVector.ColumnValues[(short)col.ColumnID]);
                        }
                    }
                    else if (physicalColumns[columnIndex] != null)
                    {
                        columnValue = ReadPhysical(record, col, physicalColumns[columnIndex], sqlType, schema);
                    }
                    else
                    {
                        var nonSparseIndex = schema.NonSparseIndexes[col.Name];
                        // Before we even try to parse the column & make a null bitmap lookup, ensure that it's present in the record.
                        // There may be columns > record.NumberOfColumns caused by nullable columns added to the schema after the record was written.
                        if (nonSparseIndex < record.NumberOfColumns && col.UnderlyingType != ColumnType.Computed)
                        {
                            if (sqlType.IsVariableLength)
                            {
                                // If there's either no null bitmap, or the null bitmap defines the column as non-null.
                                if (!record.HasNullBitmap || !record.NullBitmap[nonSparseIndex])
                                {
                                    // Trailing empty columns may be beyond the stored slots or occupy a forwarded
                                    // record's back-pointer slot, which has no data proxy. Both mean empty, not null.
                                    if (variableColumnIndex < record.NumberOfVariableLengthColumns &&
                                        record.VariableLengthColumnData.TryGetValue(variableColumnIndex, out var dataProxy))
                                    {
                                        if (schema.ShouldDefer(col))
                                            columnValue = new DeferredColumnValue(dataProxy, sqlType);
                                        else
                                        {
                                            var data = dataProxy.GetBytes()?.ToArray();
                                            columnValue = sqlType.GetValue(data ?? new byte[0]);
                                        }
                                    }
                                    else
                                        columnValue = sqlType.GetValue(new byte[0]);
                                }

                                variableColumnIndex++;
                            }
                            else
                            {
                                // Must cache type FixedLength as it may change after getting a value (e.g. SqlBit)
                                var fixedLength = sqlType.FixedLength.Value;

                                if ((!record.HasNullBitmap || !record.NullBitmap[nonSparseIndex]) && col.UnderlyingType != ColumnType.Bit)
                                {
                                    var valueBytes = record.FixedLengthData.Skip(fixedOffset).Take(fixedLength).ToArray();

                                    // We may run out of fixed length bytes. In certain conditions a null integer may have been added without
                                    // there being a null bitmap. In such a case, we detect the null condition by there not being enough fixed
                                    // length bytes to process.
                                    if (valueBytes.Length > 0)
                                    {
                                        columnValue = sqlType.GetValue(valueBytes);
                                    }
                                }
                                else if(col.UnderlyingType == ColumnType.Bit && !schema.IsDroppedColumn(col))
                                {
                                    if (readState.IsFirstBit)
                                        bitColumnBytes = record.FixedLengthData.Skip(fixedOffset).Take(fixedLength).ToArray();

                                    var value = sqlType.GetValue(bitColumnBytes);
                                    columnValue = !record.HasNullBitmap || !record.NullBitmap[nonSparseIndex] ? value : null;
                                }

                                fixedOffset += fixedLength;
                            }
                        }
                        else if (col.UnderlyingType == ColumnType.Computed)
                        {
                            columnValue = sqlType.GetValue(null);
                        }
                        else if(!col.IsNullable)
                        {
                            columnValue = schema.GetDefaultValue(col, sqlType);
                        }
                    }

                    if(!schema.IsDroppedColumn(col))
                        dataRow[col] = columnValue;
                }

                yield return dataRow;
            }
        }

        /// <summary>
        /// The physical location of a column, read once from the catalog instead of on every row (each property of
        /// SystemInternalsPartitionColumn is a lookup by name).
        /// </summary>
        private sealed class PhysicalColumn
        {
            public readonly bool IsDropped;
            public readonly short LeafOffset;
            public readonly short LeafNullBit;
            public readonly byte LeafBitPosition;
            public readonly short MaxInrowLength;

            public PhysicalColumn(SystemInternalsPartitionColumn column, bool droppedInSchema)
            {
                IsDropped = column.IsDropped || droppedInSchema;
                LeafOffset = column.LeafOffset;
                LeafNullBit = column.LeafNullBit;
                LeafBitPosition = column.LeafBitPosition;
                MaxInrowLength = column.MaxInrowLength;
            }
        }

        /// <summary>
        /// Reads a column using its physical location from sysrscols instead of deriving it from the column order:
        /// fixed-length columns at leaf_offset, variable-length columns at slot -leaf_offset - 1, the null bit at
        /// leaf_null_bit and bit columns at leaf_bit_position.
        /// </summary>
        private static object ReadPhysical(PrimaryRecord record, DataColumn col, PhysicalColumn physical, ISqlType sqlType, DataExtractorHelper schema)
        {
            // A dropped column (or the old copy of a column altered in place, which shares its bytes with the
            // live column) is never decoded.
            if (physical.IsDropped)
                return null;

            if (col.UnderlyingType == ColumnType.Computed)
                return sqlType.GetValue(null);

            // Column added after the record was written: it isn't stored in the record.
            int nullIndex = physical.LeafNullBit - 1;
            if (nullIndex >= record.NumberOfColumns)
                return col.IsNullable ? null : schema.GetDefaultValue(col, sqlType);

            if (record.HasNullBitmap && record.NullBitmap[nullIndex])
                return null;

            if (physical.LeafOffset < 0)
            {
                int variableIndex = -physical.LeafOffset - 1;
                // Not stored: beyond the record's variable-length columns, or the slot of a forwarded record's back
                // pointer (its last variable-length column, which isn't data) when its trailing columns are empty.
                if (variableIndex >= record.NumberOfVariableLengthColumns || !record.VariableLengthColumnData.ContainsKey(variableIndex))
                    return sqlType.GetValue(new byte[0]);

                if (schema.ShouldDefer(col))
                    return new DeferredColumnValue(record.VariableLengthColumnData[variableIndex], sqlType);

                var data = record.VariableLengthColumnData[variableIndex].GetBytes();
                return sqlType.GetValue(data as byte[] ?? data?.ToArray() ?? new byte[0]);
            }

            // leaf_offset counts from the start of the record; FixedLengthData starts after the 4 byte header.
            int start = physical.LeafOffset - 4;

            if (col.UnderlyingType == ColumnType.Bit)
            {
                if (start >= record.FixedLengthData.Length)
                    return null;

                return ((record.FixedLengthData[start] >> physical.LeafBitPosition) & 1) == 1;
            }

            int length = physical.MaxInrowLength;
            if (start + length > record.FixedLengthData.Length)
                return null;

            var bytes = new byte[length];
            System.Array.Copy(record.FixedLengthData, start, bytes, 0, length);
            return sqlType.GetValue(bytes);
        }

        internal override PagePointer NextPage => page.Header.NextPage;
    }
}
