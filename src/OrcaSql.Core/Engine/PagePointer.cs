using System;
using System.Diagnostics;

namespace OrcaSql.Core.Engine
{
	public class PagePointer
	{
		public short FileID;
		public long PageID;

		public static readonly PagePointer Zero = new PagePointer(0, 0);

		[DebuggerStepThrough]
		public PagePointer(short fileID, long pageID)
		{
			FileID = fileID;
			PageID = pageID;
		}

		public PagePointer(byte[] bytes)
		{
			if (bytes.Length != 6)
				throw new ArgumentException("Input must be 6 bytes in the format pageID(4)fileID(2).");

			PageID = BitConverter.ToInt32(bytes, 0);
			FileID = BitConverter.ToInt16(bytes, 4);
		}

		public static bool operator ==(PagePointer a, PagePointer b)
		{
            return a?.PageID == b?.PageID && a?.FileID == b?.FileID;
        }

		public static bool operator !=(PagePointer a, PagePointer b)
        {
            return a?.PageID != b?.PageID || a?.FileID != b?.FileID;
		}

		public override bool Equals(object obj)
        {
            if (obj == null) return false;
			var b = (PagePointer)obj;

			return b.FileID == FileID && b.PageID == PageID;
		}

		public override int GetHashCode()
		{
			// KISS
			return (FileID + "_" + PageID).GetHashCode();
		}

		[DebuggerStepThrough]
		public override string ToString()
		{
			return "(" + FileID + ":" + PageID + ")";
		}
	}
}