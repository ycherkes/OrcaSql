using NUnit.Framework;
using OrcaSql.Core.Engine;
using OrcaSql.Core.Engine.Pages;

namespace OrcaSql.Core.Tests.Engine.Pages
{
	[TestFixture]
	public class SgamPageTests
	{
		[Test]
		public void GetSgamPointerForPage()
		{
			Assert.AreEqual(new PagePointer(1, 3), SgamPage.GetSgamPointerForPage(new PagePointer(1, 27)));
			Assert.AreEqual(new PagePointer(1, 3), SgamPage.GetSgamPointerForPage(new PagePointer(1, 0)));
			Assert.AreEqual(new PagePointer(1, 511233), SgamPage.GetSgamPointerForPage(new PagePointer(1, 511232)));
			Assert.AreEqual(new PagePointer(1, 511233), SgamPage.GetSgamPointerForPage(new PagePointer(1, 511233)));
			Assert.AreEqual(new PagePointer(1, 511233), SgamPage.GetSgamPointerForPage(new PagePointer(1, 511234)));
		}
	}
}