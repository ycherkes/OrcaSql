using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.Engine.Pages;

namespace OrcaSql.Core.Tests.Engine.Pages
{
	[TestFixture]
	public class GamPageTests
	{
		[Test]
		public void GetGamPointerForPage()
		{
			Assert.AreEqual(new PagePointer(1, 2), GamPage.GetGamPointerForPage(new PagePointer(1, 27)));
			Assert.AreEqual(new PagePointer(1, 2), GamPage.GetGamPointerForPage(new PagePointer(1, 0)));
			Assert.AreEqual(new PagePointer(1, 2), GamPage.GetGamPointerForPage(new PagePointer(1, 511231)));
			Assert.AreEqual(new PagePointer(1, 511232), GamPage.GetGamPointerForPage(new PagePointer(1, 511232)));
			Assert.AreEqual(new PagePointer(1, 511232), GamPage.GetGamPointerForPage(new PagePointer(1, 511233)));
		}
	}
}