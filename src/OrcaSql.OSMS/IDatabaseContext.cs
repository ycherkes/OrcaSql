using OrcaSql.Core.Engine;

namespace OrcaSql.OSMS
{
    public interface IDatabaseContext
    {
        Database Database { get; }
    }
}