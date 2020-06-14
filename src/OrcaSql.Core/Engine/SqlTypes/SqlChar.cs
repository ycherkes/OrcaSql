using System;
using System.Text;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public class SqlChar : SqlTypeBase
	{
		private readonly short length;
        private readonly Encoding _encoding;

        public SqlChar(short length, Encoding encoding, CompressionContext compression)
			: base(compression)
        {
            this.length = length;
            _encoding = encoding;
        }

		public override bool IsVariableLength
		{
			get { return false; }
		}

		public override short? FixedLength
		{
			get { return length; }
		}

		public override object GetValue(byte[] value)
		{
			if (CompressionContext.CompressionLevel != CompressionLevel.None)
			{
				if (value.Length > length)
					throw new ArgumentException("Invalid value length: " + value.Length);

                return _encoding.GetString(value);
            }

            if (value.Length != length)
                throw new ArgumentException("Invalid value length: " + value.Length);

            return _encoding.GetString(value);
        }

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            var bracketLessString = columnConstraint.Definition.Trim('(', ')');
            return bracketLessString.StartsWith("'") ? bracketLessString.Trim('\'') : null;
        }
    }
}