using System;
using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.Engine.Pages.PFS;
using OrcaSql.Core.Engine.Records.Parsers;
using OrcaSql.Core.MetaData;
using OrcaSql.Core.MetaData.Enumerations;

namespace OrcaSql.Core.Engine
{
	public class DataScanner : Scanner
	{
		public DataScanner(Database database)
			: base(database)
		{ }

		/// <summary>
		/// Will scan any table - heap or clustered - and return an IEnumerable of generic rows with data & schema
		/// </summary>
		public IEnumerable<Row> ScanTable(string tableName, int? schemaId = null, bool isSysTable = true)
		{
			var schema = MetaData.GetEmptyDataRow(tableName, schemaId);

			return ScanTable(tableName, schema, isSysTable);
		}

        public DataRow GetEmptyDataRow(string tableName, int? schemaId = null)
        {
            var schema = MetaData.GetEmptyDataRow(tableName, schemaId);

            return schema;
        }

        /// <summary>
        /// Will scan any table - heap or clustered - and return an IEnumerable of typed rows with data & schema
        /// </summary>
        internal IEnumerable<TDataRow> ScanTable<TDataRow>(string tableName) where TDataRow : Row, new()
		{
			var schema = new TDataRow();

			return ScanTable(tableName, schema).Cast<TDataRow>();
		}

		/// <summary>
		/// Scans a linked list of pages returning an IEnumerable of typed rows with data & schema
		/// </summary>
		internal IEnumerable<TDataRow> ScanLinkedDataPages<TDataRow>(PagePointer loc, CompressionContext compression) where TDataRow : Row, new()
		{
			return ScanLinkedDataPages(loc, new DataExtractorHelper(new TDataRow()), compression, true).Cast<TDataRow>();
		}

		/// <summary>
		/// Starts at the data page (loc) and follows the NextPage pointer chain till the end.
		/// </summary>
		internal IEnumerable<Row> ScanLinkedDataPages(PagePointer loc, DataExtractorHelper schema,
            CompressionContext compression, bool isSysTable)
		{
			while (PagePointer.Zero != loc && loc != null && loc.PageID > 0)
			{
				var recordParser = RecordEntityParser.CreateEntityParserForPage(loc, compression, Database, isSysTable);
				
				foreach (var dr in recordParser.GetEntities(schema))
					yield return dr;

				loc = recordParser.NextPage;
			}
		}

		private IEnumerable<Row> ScanTable(string tableName, Row schema, bool isSysTable = true)
		{
			// Get object
			var tableObject = Database.BaseTables.SysSchObjs
				.Where(x => x.name == tableName)
				.SingleOrDefault(x => x.type.Trim() == ObjectType.INTERNAL_TABLE || x.type.Trim() == ObjectType.SYSTEM_TABLE || x.type.Trim() == ObjectType.USER_TABLE);

			if (tableObject == null)
				throw new ArgumentException("Table does not exist.");

			// Get rowset, prefer clustered index if exists
			var partitions = Database.Dmvs.Partitions
				.Where(x => x.ObjectID == tableObject.id && x.IndexID <= 1)
				.OrderBy(x => x.PartitionNumber)
                .ToArray();

			if (!partitions.Any())
				throw new ArgumentException("Table has no partitions.");

			// Loop all partitions and return results one by one
			return partitions.SelectMany(partition => ScanPartition(partition.PartitionID, partition.PartitionNumber, schema, isSysTable));
		}

		private IEnumerable<Row> ScanPartition(long partitionID, int partitionNumber, Row schema, bool isSysTable = true)
		{
			// Lookup partition
			var partition = Database.Dmvs.Partitions
				.SingleOrDefault(p => p.PartitionID == partitionID && p.PartitionNumber == partitionNumber);

			if(partition == null)
				throw new ArgumentException("Partition (" + partitionID + "." + partitionNumber + " does not exist.");

			// Get allocation unit for in-row data
			var au = Database.Dmvs.SystemInternalsAllocationUnits
				.SingleOrDefault(x => x.ContainerID == partition.PartitionID && x.Type == 1);

			if (au == null)
				throw new ArgumentException("Partition (" + partition.PartitionID + "." + partition.PartitionNumber + " has no HOBT allocation unit.");

			// Before we can scan either heaps or indices, we need to know the compression level as that's set at the partition level, and not at the record/page level.
			// We also need to know whether the partition is using vardecimals.
			var compression = new CompressionContext((CompressionLevel)partition.DataCompression, MetaData.PartitionHasVardecimalColumns(partition.PartitionID));

            var clusteredIndex = isSysTable ? null : Database.Dmvs.Indexes.SingleOrDefault(x => x.ObjectID == partition.ObjectID && x.Type == 1);

            var useClusteredIndex = isSysTable || clusteredIndex != null;

            var partitionColumns = isSysTable ? null : Database.Dmvs.SystemInternalsPartitionColumns.Where(x => x.PartitionID == partition.PartitionID).ToArray();

            var defaultConstraints = isSysTable ? null : Database.Dmvs.SysDefaultConstraints.Where(x => x.ParentObjectId == partition.ObjectID).ToArray();

            var schemaWrapper = new DataExtractorHelper(schema, Database.Dmvs, null, partitionColumns, defaultConstraints);

            // Heap tables won't have root pages, thus we can check whether a root page is defined for the HOBT allocation unit
            if (au.RootPagePointer != PagePointer.Zero && useClusteredIndex)
            {
                var currentPage = isSysTable ? au.FirstPagePointer : au.RootPagePointer;

                if (currentPage != au.FirstPagePointer)
                {
                    while (true)
                    {
                        var ciPage = Database.GetClusteredIndexPage(currentPage, isSysTable);

                        currentPage = ciPage.Records.Select(x => x.PageId).FirstOrDefault();

                        if (ciPage.Header.Level <= 1)
                        {
                            break;
                        }
                    }
                }

                // Index
                foreach (var row in ScanLinkedDataPages(currentPage, schemaWrapper, compression, isSysTable))
                    yield return row;
            }
            else
            {
				// Heap
				foreach (var row in ScanHeap(au.FirstIamPagePointer, schemaWrapper, compression, isSysTable))
					yield return row;
			}
		}

		/// <summary>
		/// Scans a heap beginning from the provided IAM page and onwards.
		/// </summary>
		private IEnumerable<Row> ScanHeap(PagePointer loc, DataExtractorHelper schema, CompressionContext compression,
            bool isSysTable)
		{
			// Traverse the linked list of IAM pages until the tail pointer is zero
			while (loc != PagePointer.Zero)
			{
				// Before scanning, check that the IAM page itself is allocated
				var pfsPage = Database.GetPfsPage(PfsPage.GetPfsPointerForPage(loc));

				// If IAM page isn't allocated, there's nothing to return
				if (!pfsPage.GetPageDescription(loc.PageID).IsAllocated)
					yield break;

				var iamPage = Database.GetIamPage(loc, isSysTable);

				// Create an array with all of the header slot pointers
				var iamPageSlots = new []
					{
						iamPage.Slot0,
						iamPage.Slot1,
						iamPage.Slot2,
						iamPage.Slot3,
						iamPage.Slot4,
						iamPage.Slot5,
						iamPage.Slot6,
						iamPage.Slot7
					};

				// Loop each header slot and yield the results, provided the header slot is allocated
				foreach (var slot in iamPageSlots.Where(x => x != PagePointer.Zero))
				{
					var recordParser = RecordEntityParser.CreateEntityParserForPage(slot, compression, Database, isSysTable);

					foreach (var dr in recordParser.GetEntities(schema))
						yield return dr;
				}

				// Then loop through allocated extents and yield results
				foreach (var extent in iamPage.GetAllocatedExtents())
				{
					// Get PFS page that tracks this extent
					var pfs = Database.GetPfsPage(PfsPage.GetPfsPointerForPage(extent.StartPage));
					
					foreach (var pageLoc in extent.GetPagePointers())
					{
						// Check if page is allocated according to PFS page
						var pfsDescription = pfs.GetPageDescription(pageLoc.PageID);

						if(!pfsDescription.IsAllocated)
							continue;

						var recordParser = RecordEntityParser.CreateEntityParserForPage(pageLoc, compression, Database, isSysTable);

						foreach (var dr in recordParser.GetEntities(schema))
							yield return dr;
					}
				}

				// Update current IAM chain location to the tail pointer
				loc = iamPage.Header.NextPage;
			}
		}
    }
}