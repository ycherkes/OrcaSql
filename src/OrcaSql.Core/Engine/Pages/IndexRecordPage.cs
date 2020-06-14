using System.Linq;
using OrcaSql.Core.Engine.Records;
using OrcaSql.Framework;

namespace OrcaSql.Core.Engine.Pages
{
	internal abstract class IndexRecordPage : RecordPage
	{
		internal IndexRecord[] Records { get; set; }

		protected IndexRecordPage(byte[] bytes, Database database)
			: base(bytes, database)
		{
			parseRecords();
		}

		private void parseRecords()
		{
			Records = new IndexRecord[Header.SlotCnt];

            //var offsets = SlotArray.OrderBy(x => x).Select((o, i) => new {o, i}).ToArray();

            //var joinedOffsets = from o1 in offsets
            //                     join o2 in offsets on o1.i equals o2.i-1 into ps
            //                     from p in ps.DefaultIfEmpty()
            //                     select new { o1.i,o1.o, length = p?.o - o1.o };
            var idx = 0;
			foreach (var recordOffset in SlotArray)
				Records[idx++] = new IndexRecord(ArrayHelper.SliceArray(RawBytes.ToArray(), recordOffset,  RawBytes.Count - recordOffset ), this);
		}
	}
}