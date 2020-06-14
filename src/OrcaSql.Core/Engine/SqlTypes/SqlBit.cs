using System;
using OrcaSql.Core.Engine.Records;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
    public class SqlBit : SqlTypeBase
    {
        private readonly RecordReadState _readState;

        public SqlBit(RecordReadState readState, CompressionContext compression)
            : base(compression)
        {
            this._readState = readState;
        }

        public override bool IsVariableLength => false;

        public override short? FixedLength => _readState.IsFirstBit ? _readState.Length : (short)0;

        public override object GetValue(byte[] value)
        {
            //todo remove this hack after sorting of showing deleted records
            if (!(value?.Length > 0)) return null;
            if (CompressionContext.CompressionLevel != CompressionLevel.None)
            {
                if (value.Length > 1)
                    throw new ArgumentException("Invalid value length: " + value.Length);

                return value.Length == 1;
            }

            _readState.LoadBitByte(value[_readState.ByteIndex]);

            return _readState.GetNextBit();
        }

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            var bracketLessString = columnConstraint.Definition.Trim('(', ')');

            return bracketLessString == "0" ? false : (bracketLessString == "1" ? true : (object)null);
        }
    }
}