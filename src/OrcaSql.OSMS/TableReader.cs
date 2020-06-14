using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using OrcaSql.Core.MetaData;

namespace OrcaSql.OSMS
{
    internal class TableReader : IDataReader
    {
        private readonly IEnumerator<Row> _rows;
        private Row _currentRow;

        public TableReader(IEnumerable<Row> rows, Row schemaRow)
        {
            _rows = rows.GetEnumerator();
            FieldCount = schemaRow.Columns.Count;
        }

        public void Dispose()
        {
        }

        public string GetName(int i)
        {
            return null;
        }

        public string GetDataTypeName(int i)
        {
            return null;
        }

        public Type GetFieldType(int i)
        {
            return null;
        }

        public object GetValue(int i)
        {
            if (_currentRow.Columns[i].Type != ColumnType.Xml) return _currentRow[_currentRow.Columns[i]];

            var binaryValue = _currentRow[_currentRow.Columns[i]];

            if (binaryValue == DBNull.Value || binaryValue == null) return binaryValue;

            using (var ms = new MemoryStream((byte[]) binaryValue))
            {
                var sqlXml = new SqlXml(ms);
                return sqlXml.Value;
            }

        }

        public int GetValues(object[] values)
        {
            return 0;
        }

        public int GetOrdinal(string name)
        {
            return 0;
        }

        public bool GetBoolean(int i)
        {
            return false;
        }

        public byte GetByte(int i)
        {
            return 0;
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return 0;
        }

        public char GetChar(int i)
        {
            return '\0';
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return 0;
        }

        public Guid GetGuid(int i)
        {
            return new Guid();
        }

        public short GetInt16(int i)
        {
            return 0;
        }

        public int GetInt32(int i)
        {
            return 0;
        }

        public long GetInt64(int i)
        {
            return 0;
        }

        public float GetFloat(int i)
        {
            return 0;
        }

        public double GetDouble(int i)
        {
            return 0;
        }

        public string GetString(int i)
        {
            return null;
        }

        public decimal GetDecimal(int i)
        {
            return 0;
        }

        public DateTime GetDateTime(int i)
        {
            return new DateTime();
        }

        public IDataReader GetData(int i)
        {
            return null;
        }

        public bool IsDBNull(int i)
        {
            return false;
        }

        public int FieldCount { get; }

        public object this[int i] => null;

        public object this[string name] => null;

        public void Close()
        {
        }

        public DataTable GetSchemaTable()
        {
            return null;
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (!_rows.MoveNext()) return false;

            _currentRow = _rows.Current;

            return true;
        }

        public int Depth { get; }
        public bool IsClosed { get; }
        public int RecordsAffected { get; }
    }
}