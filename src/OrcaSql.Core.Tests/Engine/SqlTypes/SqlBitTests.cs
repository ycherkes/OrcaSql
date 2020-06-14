using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.Engine.Records;
using OrcaSql.Core.Engine.SqlTypes;

namespace OrcaSql.Core.Tests.Engine.SqlTypes
{
	[TestFixture]
	public class SqlBitTests
	{
		[Test]
		public void GetValue()
		{
			var readState = new RecordReadState(1);
			var type = new SqlBit(readState, CompressionContext.NoCompression);

			// No bytes read - length is one
			Assert.AreEqual(1, type.FixedLength);

			// Load byte and check length is 0
			readState.LoadBitByte(0xD2);
			Assert.AreEqual(0, type.FixedLength);

			Assert.IsFalse((bool)type.GetValue(new byte[0]));
			Assert.IsTrue((bool)type.GetValue(new byte[0]));
			Assert.IsFalse((bool)type.GetValue(new byte[0]));
			Assert.IsFalse((bool)type.GetValue(new byte[0]));
			Assert.IsTrue((bool)type.GetValue(new byte[0]));
			Assert.IsFalse((bool)type.GetValue(new byte[0]));
			Assert.IsTrue((bool)type.GetValue(new byte[0]));

			// One bit left - length should still be 0
			Assert.AreEqual(0, type.FixedLength);

			Assert.IsTrue((bool)type.GetValue(new byte[0]));

			// All bits consumed - length should be 1
			Assert.AreEqual(1, type.FixedLength);
		}
	}
}