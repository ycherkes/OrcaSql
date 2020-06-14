using System;
using System.Collections.Generic;
using OrcaSql.Core.MetaData;

namespace OrcaSql.Core.Engine.Records.Parsers
{
	internal abstract class RecordEntityParser
	{
		internal abstract IEnumerable<Row> GetEntities(DataExtractorHelper schema);
		internal abstract PagePointer NextPage { get; }
		
		internal static RecordEntityParser CreateEntityParserForPage(PagePointer loc, CompressionContext compression, Database database, bool isSysTable = true)
		{
			switch (compression.CompressionLevel)
			{
				case CompressionLevel.Page:
					throw new NotImplementedException("Page compression not yet supported.");

				case CompressionLevel.Row:
					return new CompressedRecordEntityParser(database.GetCompressedRecordPage(loc, compression));

				case CompressionLevel.None:
					return new PrimaryRecordEntityParser(database.GetPrimaryRecordPage(loc, compression, isSysTable), compression);

				default:
					throw new ArgumentException("Unsupported compression level: " + compression.CompressionLevel);
			}
		}
	}
}