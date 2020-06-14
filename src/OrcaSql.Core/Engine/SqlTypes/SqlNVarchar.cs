using System.Text;
using OrcaSql.Core.MetaData.DMVs;
using OrcaSql.Framework.SCSU;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public class SqlNVarchar : SqlTypeBase
	{
		public SqlNVarchar(CompressionContext compression)
			: base(compression)
		{ }

		public override bool IsVariableLength
		{
			get { return true; }
		}

		public override short? FixedLength
		{
			get { return null; }
		}

		public override object GetValue(byte[] value)
		{
			if (CompressionContext.CompressionLevel != CompressionLevel.None)
			{
				var expander = new ScsuExpander();
				return expander.Expand(value);
			}
			else
			{
				return Encoding.Unicode.GetString(value);
			}
		}

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            var bracketLessString = columnConstraint.Definition.Trim('(', ')');
            return bracketLessString.StartsWith("'") ? bracketLessString.Trim('\'') : null;
        }
    }
}