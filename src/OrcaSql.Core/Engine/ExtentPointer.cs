using System.Collections.Generic;

namespace OrcaSql.Core.Engine
{
	public class ExtentPointer
	{
		public PagePointer StartPage { get; private set; }
		public PagePointer EndPage { get; private set; }

		public ExtentPointer(PagePointer start)
		{
			StartPage = start;
			EndPage = new PagePointer(start.FileID, start.PageID + 7);
		}

		public IEnumerable<PagePointer> GetPagePointers()
		{
			for (var pageID = StartPage.PageID; pageID <= EndPage.PageID; pageID++)
				yield return new PagePointer(StartPage.FileID, pageID);
		}

		public override string ToString()
		{
			return StartPage + " - " + EndPage;
		}
	}
}