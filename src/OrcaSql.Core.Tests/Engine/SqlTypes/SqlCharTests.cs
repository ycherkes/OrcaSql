using System;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData.TableValuedDictionaries;

namespace OrcaSql.Core.Tests.Engine.SqlTypes
{
	[TestFixture]
	public class SqlCharTests
    {
        private static Encoding encoding =
            Encoding.GetEncoding(new CultureInfo(Collations.GetByCollationId(872468488).Lcid).TextInfo.ANSICodePage);

        [Test]
		public void GetValue()
		{
			var type = new SqlChar(5, encoding, CompressionContext.NoCompression);
			byte[] input;

			input = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
			Assert.AreEqual("Hello", type.GetValue(input));
		}

		[Test]
		public void Length()
		{
			var type = new SqlChar(5, encoding, CompressionContext.NoCompression);

			Assert.Throws<ArgumentException>(() => type.GetValue(new byte[4]));
			Assert.Throws<ArgumentException>(() => type.GetValue(new byte[6]));
		}
	}
}