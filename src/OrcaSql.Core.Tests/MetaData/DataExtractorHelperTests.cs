using System;
using System.Collections.Generic;
using NUnit.Framework;
using OrcaSql.Core.MetaData;

namespace OrcaSql.Core.Tests.MetaData
{
	[TestFixture]
	public class DataExtractorHelperTests
	{
		[Test]
		public void ShouldDeferMatchesColumnNamesCaseInsensitively()
		{
			var column = new DataColumn("Payload", "varchar(max)");
			var row = new DataRow(new[] { column });
			var helper = new DataExtractorHelper(row, new HashSet<string> { "payload" });

			Assert.IsTrue(helper.ShouldDefer(column));
		}

		[Test]
		public void ConstructorRejectsUnknownDeferredColumns()
		{
			var row = new DataRow(new[] { new DataColumn("Payload", "varchar(max)") });

			Assert.Throws<ArgumentException>(() => new DataExtractorHelper(row, new HashSet<string> { "Missing" }));
		}

		[Test]
		public void ConstructorRejectsNonVariableDeferredColumns()
		{
			var row = new DataRow(new[] { new DataColumn("Id", "int") });

			Assert.Throws<ArgumentException>(() => new DataExtractorHelper(row, new HashSet<string> { "Id" }));
		}

		[Test]
		public void ConstructorRejectsSparseDeferredColumns()
		{
			var column = new DataColumn("SparsePayload", "varchar(max)") { IsSparse = true };
			var row = new DataRow(new[] { column });

			Assert.Throws<ArgumentException>(() => new DataExtractorHelper(row, new HashSet<string> { "SparsePayload" }));
		}

		[Test]
		public void ConstructorRejectsComputedDeferredColumns()
		{
			var row = new DataRow(new[] { new DataColumn("Calc", "Computed") });

			Assert.Throws<ArgumentException>(() => new DataExtractorHelper(row, new HashSet<string> { "Calc" }));
		}
	}
}
