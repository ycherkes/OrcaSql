using System.Globalization;
using System.Text;
using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData.TableValuedDictionaries;

namespace OrcaSql.Core.Tests.Engine.SqlTypes
{
	[TestFixture]
	public class SqlVarcharTests
	{
        private static Encoding encoding =
            Encoding.GetEncoding(new CultureInfo(Collations.GetByCollationId(872468488).Lcid).TextInfo.ANSICodePage);

        [Test]
		public void GetValue()
		{
			var type = new SqlVarchar(CompressionContext.NoCompression, encoding);
			byte[] input;

			input = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
			Assert.AreEqual("Hello", (string)type.GetValue(input));
		}
	}
}