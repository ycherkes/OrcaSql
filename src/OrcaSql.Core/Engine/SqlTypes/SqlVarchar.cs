using System.Text;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public class SqlVarchar : SqlTypeBase
	{
        private readonly Encoding _encoding;

        public SqlVarchar(CompressionContext compression, Encoding encoding)
			: base(compression)
        {
            _encoding = encoding;
        }

		public override bool IsVariableLength => true;

        public override short? FixedLength => null;

        public override object GetValue(byte[] value)
		{
			return _encoding.GetString(value);
		}

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            var bracketLessString = columnConstraint.Definition.Trim('(', ')');
            return bracketLessString.StartsWith("'") ? bracketLessString.Trim('\'') : null;
        }
    }
}