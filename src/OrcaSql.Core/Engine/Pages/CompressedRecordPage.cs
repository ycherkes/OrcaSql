using System.Linq;
using OrcaSql.Core.Engine.Records.Compression;
using OrcaSql.Framework;

namespace OrcaSql.Core.Engine.Pages
{
	internal class CompressedRecordPage : RecordPage
	{
		internal CompressedRecord[] Records { get; set; }

		protected CompressionContext CompressionContext;

		internal CompressedRecordPage(byte[] bytes, CompressionContext compression, Database database)
			: base(bytes, database)
		{
			CompressionContext = compression;

			parseRecords();
		}

		private void parseRecords()
		{
			Records = new CompressedRecord[Header.SlotCnt];

			int cnt = 0;
			foreach (short recordOffset in SlotArray)
				Records[cnt++] = new CompressedRecord(ArrayHelper.SliceArray(RawBytes.ToArray(), recordOffset, RawBytes.Count - recordOffset), this);
		}
	}
}