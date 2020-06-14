using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.Engine.SqlTypes;

namespace OrcaSql.Core.Tests.Engine.SqlTypes
{
	[TestFixture]
	public class SqlVarBinaryTests
	{
		[Test]
		public void GetValue()
		{
			var type = new SqlVarBinary(CompressionContext.NoCompression);
			byte[] input;

			input = new byte[] { 0x25, 0xF8, 0x32 };
			Assert.AreEqual(new byte[] { 0x25, 0xF8, 0x32 }, type.GetValue(input));
		}
	}
}