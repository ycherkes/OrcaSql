using System.Collections.Generic;
using System.Linq;
using OrcaSql.Core.Engine.Pages;
using OrcaSql.Core.Engine.Records.VariableLengthDataProxies;
using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData;

namespace OrcaSql.Core.Engine.Records.Parsers
{
	internal class CompressedRecordEntityParser : RecordEntityParser
	{
		private readonly CompressedRecordPage page;

		internal CompressedRecordEntityParser(CompressedRecordPage page)
		{
			this.page = page;
		}

		internal override IEnumerable<Row> GetEntities(DataExtractorHelper schema)
		{
			foreach (var record in page.Records)
			{
				var dataRow = schema.NewRow();
				var readState = new RecordReadState(schema.Columns.Count(x => x.UnderlyingType == ColumnType.Bit));

				int columnIndex = 0;
				foreach (DataColumn col in dataRow.Columns)
				{
					var sqlType = SqlTypeFactory.Create(col, readState, new CompressionContext(CompressionLevel.Row, true));

					IVariableLengthDataProxy dataProxy = record.GetPhysicalColumnBytes(columnIndex);

					if (dataProxy == null)
						dataRow[col] = null;
					else
						dataRow[col] = sqlType.GetValue(dataProxy.GetBytes().ToArray());

					columnIndex++;
				}

				yield return dataRow;
			}
		}

		internal override PagePointer NextPage
		{
			get { return page.Header.NextPage; }
		}
	}
}