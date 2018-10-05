using System;
using System.IO;
using B2Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using B2Net.Models;
using System.Linq;
using System.Threading.Tasks;
using B2.Net.Tests;

namespace B2Net.Tests {
	[TestClass]
	public class BucketRestrictedTests : BaseTest {
		private string RestrictedBucketName = "B2Net-Test";
		private string BucketName = "";
		
        [TestMethod]
		[ExpectedException(typeof(B2Exception), "Unauthorized error when operating on Buckets. Are you sure the key you are using has access? ")]
		public async Task GetBucketListTest() {
			// Key that is restricted to RestrictedBucketName above.
	        var client = new B2Client(B2Client.Authorize(new B2Options() {
		        AccountId = TestConstants.AccountId,
		        KeyId = "00151189a8b4c7a0000000006",
		        ApplicationKey = "K001+GGkBNcbJVj3LD4+e3s5pCUMQ7U"
	        }));
	        BucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "").Substring(0, 6)}";

			var bucket = await client.Buckets.Create(BucketName, BucketTypes.allPrivate);
		}

		[TestMethod]
		[ExpectedException(typeof(AuthorizationException), "You supplied an application keyid, but not the accountid. Both are required if you are not using a master key.")]
		public void BadInitialization() {
			// Missing AccountId
			var client = new B2Client(B2Client.Authorize(new B2Options() {
				KeyId = "00151189a8b4c7a0000000006",
				ApplicationKey = "K001+GGkBNcbJVj3LD4+e3s5pCUMQ7U"
			}));
		}
	}
}
