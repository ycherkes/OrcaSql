using System;
using System.Reflection;
using NUnit.Framework;
using OrcaSql.Core.Engine.Records;

namespace OrcaSql.Core.Tests.Engine.Records
{
	[TestFixture]
	public class RecordParserCompatibilityTests
	{
		[TestCase("ParseVariableLengthColumns", typeof(void))]
		[TestCase("ParseNullBitmap", typeof(short))]
		public void OriginalProtectedParserSignatureIsAvailable(string name, Type returnType)
		{
			// Compiled subclasses reference this exact signature; an optional third parameter cannot replace it.
			var method = typeof(Record).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic,
				null, new[] { typeof(byte[]), typeof(short).MakeByRefType() }, null);

			Assert.IsNotNull(method, "The original two-parameter {0} method must remain available.", name);
			Assert.IsTrue(method.IsFamily);
			Assert.AreEqual(returnType, method.ReturnType);
		}
	}
}
