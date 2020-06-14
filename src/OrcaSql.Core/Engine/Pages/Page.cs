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
			RawBytes = bytes;
			Header = new PageHeader(RawHeader);
		}

		public byte[] RawHeader => RawBytes.Take(96).ToArray();

        public byte[] RawBody => RawBytes.Skip(96).ToArray();

        public override string  ToString()
		{
			return "{" + Header.Type + " (" + Header.Pointer.FileID + ":" + Header.Pointer.PageID + ")}";
		}
	}
}