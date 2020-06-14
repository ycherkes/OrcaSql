namespace OrcaSql.RawCore.Types
{
	public interface IRawFixedLengthType : IRawType
	{
		short Length { get; }
	}
}