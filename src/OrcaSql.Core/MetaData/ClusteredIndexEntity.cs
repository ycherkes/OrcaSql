using OrcaSql.Core.Engine;

namespace OrcaSql.Core.MetaData
{
	public abstract class ClusteredIndexEntity
	{
		public PagePointer ChildPage { get; set; }
	}
}