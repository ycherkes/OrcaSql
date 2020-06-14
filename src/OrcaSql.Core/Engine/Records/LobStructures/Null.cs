using System;

namespace OrcaSql.Core.Engine.Records.LobStructures
{
	/* INTERNAL (type: 2)
	 * 
	 * Byte		Content
	 * 0-7		Blob ID (long)
	 * 8-9		Type (short)
	 * 10-11	MaxLinks (short)
	 * 12-13	CurLinks (short)
	 * 14-15	Level (short)
	 * 16-23	Offset[0] (long)
	 * 24-27	PageID[0] (int)
	 * 28-29	FileID[0] (short)
	 * 30-31	SlotID[0] (short)
	 * ...
	*/
	public class Null : LobStructureBase, ILobStructure
	{
		public long BlobID { get; private set; }
		public short MaxLinks { get; private set; }
		public short CurLinks { get; private set; }
		public short Level { get; private set; }

		public Null(byte[] bytes, Database database)
			: base(database)
		{
			short type = BitConverter.ToInt16(bytes, 8);
			if (type != (short)LobStructureType.NULL)
				throw new ArgumentException("Invalid byte structure. Expected NULL, found " + type);

			BlobID = BitConverter.ToInt64(bytes, 0);
			MaxLinks = BitConverter.ToInt16(bytes, 10);
			CurLinks = BitConverter.ToInt16(bytes, 12);
			Level = BitConverter.ToInt16(bytes, 14);
		}

		public byte[] GetData()
        {
            return null;
        }
	}
}