using System;
using System.Collections.Generic;
using System.Linq;

namespace OrcaSql.Core.Engine.Pages
{
	public class Page
	{
		public Database Database { get; private set; }
		public PageHeader Header;

		// Raw content of the page (8192 bytes)
		public IReadOnlyCollection<byte> RawBytes { get; private set; }

		public Page(byte[] bytes, Database database)
		{
			if (bytes.Length != 8192)
				throw new ArgumentException("bytes");

			Database = database;
			Bytes = bytes;
			RawBytes = bytes;
			Header = new PageHeader(RawHeader);
		}

		// The same 8192 bytes as RawBytes, without copying them on every access.
		internal byte[] Bytes { get; }

		public byte[] RawHeader => Framework.ArrayHelper.SliceArray(Bytes, 0, 96);

        public byte[] RawBody => RawBytes.Skip(96).ToArray();

        public override string  ToString()
		{
			return "{" + Header.Type + " (" + Header.Pointer.FileID + ":" + Header.Pointer.PageID + ")}";
		}
	}
}