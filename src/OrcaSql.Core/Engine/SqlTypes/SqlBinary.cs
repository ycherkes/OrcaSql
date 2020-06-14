using System;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Engine.SqlTypes
{
	public class SqlBinary : SqlTypeBase
	{
		private readonly short length;

		public SqlBinary(short length, CompressionContext compression)
			: base(compression)
		{
			this.length = length;
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

				if (value.Length < length)
				{
					var result = new byte[length];
					Array.Copy(value, result, value.Length);

					return result;
				}
				else
					return value;
			}
			else
			{
				if (value.Length != length)
					throw new ArgumentException("Invalid value length: " + value.Length);

				return value;
			}
		}

        public override object GetDefaultValue(SysDefaultConstraint columnConstraint)
        {
            var hexString = TrimStart(columnConstraint.Definition.Trim('(', ')').TrimStart(), "0x");
            try
            {
                return StringToByteArray(hexString);
            }
            catch
            {
                return null;
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static string TrimStart(string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
    }
}