using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OrcaSql.Framework;

namespace OrcaSql.RawCore.Records
{
	public class RawPrimaryRecord : RawRecord
	{
		public bool IsGhostForwardedRecord { get; private set; }

		internal byte RawStatusByteB { get; private set; }

		public RawPrimaryRecord(ArrayDelimiter<byte> bytes) : base(bytes)
		{
			// Status byte B
			RawStatusByteB = bytes[1];
			IsGhostForwardedRecord = (bytes[1] & 0x01) == 0x01; // First bit

			// Fixed length size
			FixedLengthSize = (short)(BitConverter.ToInt16(bytes.SourceArray, bytes.Offset + 2) - 4);

			// Fixed length data
			FixedLengthData = new ArrayDelimiter<byte>(bytes.SourceArray, bytes.Offset + 4, FixedLengthSize);

			// Null bitmap column count
			NullBitmapColumnCount = BitConverter.ToInt16(bytes.SourceArray, bytes.Offset + 4 + FixedLengthSize);

			// Null bitmap
			NullBitmapRawBytes = new ArrayDelimiter<byte>(bytes.SourceArray, bytes.Offset + 4 + FixedLengthSize + 2, (NullBitmapColumnCount + 7) / 8);
			NullBitmap = new BitArray(NullBitmapRawBytes.ToArray());

			// Variable length offset array
			if (HasVariableLengthColumns)
			{
				int endOfNullBitmapPointer = 4 + FixedLengthSize + 2 + NullBitmapRawBytes.Count;
				
				// Number of pointers
				NumberOfVariableLengthOffsetArrayEntries = BitConverter.ToInt16(bytes.SourceArray, bytes.Offset + endOfNullBitmapPointer);
				
				// Pointers
				VariableLengthOffsetArray = new List<short>();
				for (int i = 0; i < NumberOfVariableLengthOffsetArrayEntries; i++)
					VariableLengthOffsetArray.Add(BitConverter.ToInt16(bytes.SourceArray, bytes.Offset + endOfNullBitmapPointer + 2 + i * 2));

				// Values
				int endOfVariableLengthOffsetArrayPointer = endOfNullBitmapPointer + 2 + NumberOfVariableLengthOffsetArrayEntries.Value * 2;
				int previousPointer = endOfVariableLengthOffsetArrayPointer;

				VariableLengthOffsetValues = new List<ArrayDelimiter<byte>>();
				foreach (short entry in VariableLengthOffsetArray)
				{
					VariableLengthOffsetValues.Add(new ArrayDelimiter<byte>(bytes.SourceArray, bytes.Offset + previousPointer, entry - previousPointer));
					previousPointer = entry;
				}
			}
		}
	}
}