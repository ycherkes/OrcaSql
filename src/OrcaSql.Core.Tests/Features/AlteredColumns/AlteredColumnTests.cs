using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.MetaData;
using OrcaSql.Core.Tests.SqlServerVersion;

namespace OrcaSql.Core.Tests.Features.AlteredColumns
{
	/// <summary>
	/// Tables whose physical layout no longer follows the column order: columns widened or dropped after rows were
	/// written, and heaps with forwarded rows. Fixed-length columns must be read at their leaf_offset in sysrscols.
	/// </summary>
	public class AlteredColumnTests : SqlServerSystemTestBase
	{
		[SqlServerTest]
		public void WidenedColumn(DatabaseVersion version)
		{
			RunDatabaseTest(version, db =>
			{
				var scanner = new DataScanner(db);
				var rows = scanner.ScanTable("WidenedColumn", null, false).OrderBy(x => x.Field<int>("A")).ToList();

				Assert.AreEqual(2, rows.Count);

				Assert.AreEqual(1, rows[0].Field<int>("A"));
				Assert.AreEqual(1234, rows[0].Field<int>("B"));
				Assert.AreEqual(new DateTime(2020, 1, 2, 3, 4, 5), rows[0].Field<DateTime>("C"));
				Assert.AreEqual(123.45m, rows[0].Field<decimal>("D"));
				Assert.AreEqual("first", rows[0].Field<string>("E"));

				Assert.AreEqual(2, rows[1].Field<int>("A"));
				Assert.AreEqual(100000, rows[1].Field<int>("B"));
				Assert.AreEqual(new DateTime(2021, 6, 7, 8, 9, 10), rows[1].Field<DateTime>("C"));
				Assert.AreEqual(1234567.89m, rows[1].Field<decimal>("D"));
				Assert.AreEqual("second", rows[1].Field<string>("E"));
			});
		}

		[SqlServerTest]
		public void ShrunkColumns(DatabaseVersion version)
		{
			RunDatabaseTest(version, db =>
			{
				var scanner = new DataScanner(db);
				var rows = scanner.ScanTable("ShrunkColumns", null, false).OrderBy(x => x.Field<int>("A")).ToList();

				Assert.AreEqual(2, rows.Count);

				Assert.AreEqual(1, rows[0].Field<int>("A"));
				Assert.AreEqual(7, rows[0].Field<short>("B"));
				Assert.AreEqual(123.45m, rows[0].Field<decimal>("C"));
				Assert.AreEqual(new DateTime(2020, 1, 2, 3, 4, 0), rows[0].Field<DateTime>("D"));
				Assert.AreEqual("abc  ", rows[0].Field<string>("E"));
				Assert.AreEqual(70, rows[0].Field<int>("F"));
				Assert.AreEqual(99, rows[0].Field<int>("G"));

				Assert.AreEqual(2, rows[1].Field<int>("A"));
				Assert.AreEqual(8, rows[1].Field<short>("B"));
				Assert.AreEqual(678.9m, rows[1].Field<decimal>("C"));
				Assert.AreEqual(new DateTime(2021, 6, 7, 8, 9, 0), rows[1].Field<DateTime>("D"));
				Assert.AreEqual("de   ", rows[1].Field<string>("E"));
				Assert.AreEqual(80, rows[1].Field<int>("F"));
				Assert.AreEqual(98, rows[1].Field<int>("G"));
			});
		}

		[SqlServerTest]
		public void HeapForwardedRecordsAreReturnedOnce(DatabaseVersion version)
		{
			RunDatabaseTest(version, db =>
			{
				var scanner = new DataScanner(db);
				AssertHeapForwardedRows(scanner.ScanTable("HeapForwarded", null, false));
			});
		}

		[SqlServerTest]
		public void HeapForwardedRecordsDefaultScan(DatabaseVersion version)
		{
			RunDatabaseTest(version, db =>
			{
				var scanner = new DataScanner(db);
				AssertHeapForwardedRows(scanner.ScanTable("HeapForwarded"));
			});
		}

		[SqlServerTest]
		public void HeapForwardedRecordsDeferredColumns(DatabaseVersion version)
		{
			RunDatabaseTest(version, db =>
			{
				var scanner = new DataScanner(db);
				var deferredColumns = new HashSet<string> { "B", "C" };
				AssertHeapForwardedRows(scanner.ScanTable("HeapForwarded", null, true, deferredColumns));
				AssertHeapForwardedRows(scanner.ScanTable("HeapForwarded", null, false, deferredColumns));
			});
		}

		private static void AssertHeapForwardedRows(IEnumerable<Row> scan)
		{
			var rows = scan.OrderBy(x => x.Field<int>("A")).ToList();

			Assert.AreEqual(3, rows.Count);
			Assert.AreEqual(new[] { 1, 2, 3 }, rows.Select(x => x.Field<int>("A")).ToArray());
			Assert.AreEqual("".PadLeft(5000, 'A'), rows[0].Field<string>("B"));
			Assert.AreEqual("", rows[0].Field<string>("C"));
			Assert.AreEqual("".PadLeft(4000, 'B'), rows[1].Field<string>("B"));
			Assert.AreEqual("", rows[1].Field<string>("C"));
			Assert.AreEqual("".PadLeft(5000, 'C'), rows[2].Field<string>("B"));
			Assert.AreEqual("", rows[2].Field<string>("C"));
		}

		protected override void RunSetupQueries(SqlConnection conn, DatabaseVersion version)
		{
			// B and D are widened after the first row is written, so they move behind C in the record
			RunQuery(@"	CREATE TABLE WidenedColumn (A int NOT NULL, B smallint, C datetime, D decimal(5,2), E varchar(10))
						INSERT INTO WidenedColumn VALUES (1, 1234, '2020-01-02T03:04:05', 123.45, 'first')
						ALTER TABLE WidenedColumn ALTER COLUMN B int
						ALTER TABLE WidenedColumn ALTER COLUMN D decimal(12,2)
						INSERT INTO WidenedColumn VALUES (2, 100000, '2021-06-07T08:09:10', 1234567.89, 'second')", conn);

			// Shrinking a column (or making it NOT NULL) keeps it at its old offset, leaving a gap before the next one
			RunQuery(@"	CREATE TABLE ShrunkColumns (A int NOT NULL, B int, C decimal(12,2), D datetime, E char(10), F int NULL, G int)
						INSERT INTO ShrunkColumns VALUES (1, 7, 123.45, '2020-01-02T03:04:00', 'abc', 70, 99)
						ALTER TABLE ShrunkColumns ALTER COLUMN B smallint
						ALTER TABLE ShrunkColumns ALTER COLUMN C decimal(9,2)
						ALTER TABLE ShrunkColumns ALTER COLUMN D smalldatetime
						ALTER TABLE ShrunkColumns ALTER COLUMN E char(5)
						ALTER TABLE ShrunkColumns ALTER COLUMN F int NOT NULL
						INSERT INTO ShrunkColumns VALUES (2, 8, 678.9, '2021-06-07T08:09:00', 'de', 80, 98)", conn);

			// Rows 1 and 3 grow and are forwarded; C is a trailing empty column
			RunQuery(@"	CREATE TABLE HeapForwarded (A int, B varchar(5000), C varchar(10))
						INSERT INTO HeapForwarded VALUES (1, REPLICATE('A', 4000), '')
						INSERT INTO HeapForwarded VALUES (2, REPLICATE('B', 4000), '')
						INSERT INTO HeapForwarded VALUES (3, REPLICATE('C', 10), '')
						UPDATE HeapForwarded SET B = REPLICATE('A', 5000) WHERE A = 1
						UPDATE HeapForwarded SET B = REPLICATE('C', 5000) WHERE A = 3", conn);
		}
	}
}
