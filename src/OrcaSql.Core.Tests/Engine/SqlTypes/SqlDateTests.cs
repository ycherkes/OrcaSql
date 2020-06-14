using System;
using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.Engine.SqlTypes;

namespace OrcaSql.Core.Tests.Engine.SqlTypes
{
	[TestFixture]
	public class SqlDateTests
	{
		[Test]
		public void GetValue()
		{
			var type = new SqlDate(CompressionContext.NoCompression);

			var input = new byte[] { 0xf6, 0x4c, 0x0b };
			Assert.AreEqual(new DateTime(2028, 09, 09), Convert.ToDateTime(type.GetValue(input)));

			input = new byte[] { 0x71, 0x5c, 0x0b };
			Assert.AreEqual(new DateTime(2039, 07, 17), Convert.ToDateTime(type.GetValue(input)));
		}

		[Test]
		public void Length()
		{
			var type = new SqlDate(CompressionContext.NoCompression);

			Assert.Throws<ArgumentException>(() => type.GetValue(new byte[2]));
			Assert.Throws<ArgumentException>(() => type.GetValue(new byte[4]));
		}
	}
}