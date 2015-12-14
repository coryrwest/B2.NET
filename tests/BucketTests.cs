using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Net.Tests {
	[TestClass]
	public class BucketTests : BaseTest {
		[TestMethod]
		public void GetBucketList() {
			var client = new B2Client(Options);

			Options = client.Authorize().Result;

			var list = client.GetBucketList().Result;

			Assert.AreNotEqual(0, list.Count);
		}
	}
}
