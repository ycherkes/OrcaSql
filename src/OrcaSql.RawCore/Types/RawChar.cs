using System.Text;

namespace OrcaSql.RawCore.Types
{
	public class RawChar : RawType, IRawFixedLengthType
	{
		public short Length { get; private set; }
		
		public RawChar(string name, short length) : base(name)
		{
			Length = length;
		}

		public override object GetValue(byte[] bytes)
		{
			return Encoding.ASCII.GetString(bytes);
		}
	}
}