using OrcaSql.Core.Engine.Pages;

namespace OrcaSql.Core.Engine.Records.VariableLengthDataProxies
{
	public class DataProxy
	{
		protected Page OriginPage;

		protected DataProxy(Page page)
		{
			OriginPage = page;
		}
	}
}