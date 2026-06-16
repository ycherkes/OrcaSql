namespace OrcaSql.Core.MetaData
{
    /// <summary>
    /// Represents a row value that can be materialized on demand.
    /// </summary>
    public interface IDeferredValue
    {
        object Force();
    }
}
