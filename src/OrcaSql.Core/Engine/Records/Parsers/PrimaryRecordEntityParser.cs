using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.Engine.Pages;
using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData;

namespace OrcaSql.Core.Engine.Records.Parsers
{
	internal class PrimaryRecordEntityParser : RecordEntityParser
	{
		private readonly PrimaryRecordPage page;
		private readonly CompressionContext compression;
        private static readonly HashSet<RecordType> _recordsToSkip;

        static PrimaryRecordEntityParser()
        {
            _recordsToSkip = new[]{ RecordType.BlobFragment , RecordType.GhostData }.ToHashSet();
        }

		internal PrimaryRecordEntityParser(PrimaryRecordPage page, CompressionContext compression)
		{
			this.page = page;
			this.compression = compression;
		}

        internal override IEnumerable<Row> GetEntities(DataExtractorHelper schema)
        {
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

                foreach (var col in schema.Columns)
                {
                    var sqlType = SqlTypeFactory.Create(col, readState, compression);
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
                                    // If the current variable length column index exceeds the number of stored
                                    // variable length columns, the value is empty by definition (that is, 0 bytes, but not null).
                                    if (variableColumnIndex < record.NumberOfVariableLengthColumns)
                                    {
                                        var data = record.VariableLengthColumnData[variableColumnIndex].GetBytes()?.ToArray();
                                        columnValue = sqlType.GetValue(data ?? new byte[0]);
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

        internal override PagePointer NextPage => page.Header.NextPage;
    }
}