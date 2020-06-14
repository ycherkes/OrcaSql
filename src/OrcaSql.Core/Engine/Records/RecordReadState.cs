namespace OrcaSql.Core.Engine.Records
{
    public class RecordReadState
    {
        private readonly int bitsCount;

        // We start out having consumed all bits as none have been read
        public int CurrentBitIndex { get; private set; }
        private byte bits;

        public RecordReadState(int bitsCount)
        {
            this.bitsCount = bitsCount;
        }

        public void LoadBitByte(byte bits)
        {
            this.bits = bits;
        }

        public bool AllBitsConsumed => CurrentBitIndex == 8;

        public short Length => (short) (bitsCount / 8 + 1);

        public bool IsFirstBit => CurrentBitIndex == 0;

        public int ByteIndex => CurrentBitIndex / 8;

        public bool GetNextBit()
        {
            return (bits & (1 << (CurrentBitIndex++ % 8))) != 0;
        }
    }
}