using System;
using OrcaSql.Core.Engine.Pages;

namespace OrcaSql.Core.Engine.Records
{
	public class PrimaryRecord : Record
	{
		public bool HasVersioningInformation { get; private set; }
		public bool IsGhostForwardedRecord { get; private set; }

		public PrimaryRecord(byte[] bytes, Page page)
			: this(bytes, 0, page)
		{ }

		/// <summary>
		/// Parses the record that begins at <paramref name="start"/> in <paramref name="bytes"/> (usually the whole
		/// page), so the page doesn't have to be copied for every record.
		/// </summary>
		public PrimaryRecord(byte[] bytes, int start, Page page)
			: base(page)
		{
			short offset = 0;

			// Parse status bits A
			parseStatusBitsA(bytes[start + offset++]);

			// TODO: Strategize this stuff to avoid ifs, switches & impersonation
			if(Type == RecordType.ForwardingStub)
			{
				// Forwarding stub only has one status byte. Remaining 8 bytes are for (PageID, FileID, Slot)
				FixedLengthData = Slice(bytes, start + 1, 8);
				
				int pageID = BitConverter.ToInt32(bytes, start + 1);
				short fileID = BitConverter.ToInt16(bytes, start + 5);
				short slot = BitConverter.ToInt16(bytes, start + 7);

				// Only the referenced record is needed, not the rest of its page.
				var forwardPageBytes = page.Database.GetPageBytes(new PagePointer(fileID, pageID));
				short slotCount = BitConverter.ToInt16(forwardPageBytes, 22);
				if (slot < 0 || slot >= slotCount)
					throw new IndexOutOfRangeException("Forwarded record slot " + slot + " does not exist on page " + fileID + ":" + pageID + ".");

				bytes = forwardPageBytes;
				start = BitConverter.ToInt16(forwardPageBytes, forwardPageBytes.Length - slot * 2 - 2);

				parseStatusBitsA(bytes[start]);

				// We'll impersonate the ForwardingStub record type that we originated from, this allows
				// the engine to distinguish BlobFragments and the records that actually reference them.
				Type = RecordType.ForwardingStub;
			}

			// Parse status bits B
			parseStatusBitsB(bytes[start + offset++]);

			// Parse fixed length size
			short fixedLengthSize = BitConverter.ToInt16(bytes, start + offset);
			fixedLengthSize -= 4;
			offset += 2;

			// Parse fixed length data
			FixedLengthData = Slice(bytes, start + offset, fixedLengthSize);
			offset += fixedLengthSize;

			// Parse number of columns
			NumberOfColumns = BitConverter.ToInt16(bytes, start + offset);
			offset += 2;

			// Parse null bitmap, if present
			if (HasNullBitmap)
				offset = ParseNullBitmap(bytes, ref offset, start);

			// Parse variable length columns, if present
			if (HasVariableLengthColumns)
				ParseVariableLengthColumns(bytes, ref offset, start);

			// Save complete record raw bytes
			SetRawBytes(bytes, start, offset);
		}

		private void parseStatusBitsA(byte bits)
		{
			// Bit 0 (versioning bit) we don't care about as it's always 0 in 2k8+

			// Bits 1-3 represents record type
			Type = (RecordType)((bits >> 1) & 7);

			// Bit 4 determines whether a null bitmap is present
			HasNullBitmap = (bits & 0x10) != 0;

			// Bit 5 determines whether there are variable length columns
			HasVariableLengthColumns = (bits & 0x20) != 0;

			// Bit 6 determines whether the row contains versioning information
			HasVersioningInformation = (bits & 0x40) != 0;

			// Bit 7 isn't used in 2k8+
		}

		private void parseStatusBitsB(byte bits)
		{
			// As the 'Ghost forwarded record' bit is the only one stored in the second byte,
			// we can simply read the whole byte value instead of extracting the first
			// bit explicitly.
			IsGhostForwardedRecord = bits == 1;
		}
	}
}