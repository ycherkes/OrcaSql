namespace OrcaSql.RawCore.Types
{
	public interface IRawType
	{
		object GetValue(byte[] bytes);
		string Name { get; }
	}
}