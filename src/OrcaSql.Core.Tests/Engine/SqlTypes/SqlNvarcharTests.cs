using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.Engine.SqlTypes;

namespace OrcaSql.Core.Tests.Engine.SqlTypes
{
	[TestFixture]
	public class SqlNvarcharTests
	{
		[Test]
		public void GetValue()
		{
			var type = new SqlNVarchar(CompressionContext.NoCompression);
			byte[] input = new byte[] { 0x47, 0x04, 0x2f, 0x04, 0xe6, 0x00 };

			Assert.AreEqual("\u0447\u042f\u00e6", (string)type.GetValue(input));
		}
	}
}