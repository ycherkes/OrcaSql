using System;
using System.Linq;

namespace OrcaSql.Core.Engine.Pages
{
	internal class RecordPage : Page
	{
		public short[] SlotArray { get; private set; }

		internal RecordPage(byte[] bytes, Database database)
			: base(bytes, database)
		{
			parseSlotArray();
		}

		private void parseSlotArray()
		{
			SlotArray = new short[Header.SlotCnt];

			for (int i = 0; i < Header.SlotCnt; i++)
				SlotArray[i] = BitConverter.ToInt16(RawBytes.ToArray(), RawBytes.Count - i * 2 - 2);
		}
	}
}