using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace B2Net.Tests {
	[TestClass]
	public class BucketRestrictedTests : BaseTest {
		private string BucketName = "";

		[TestMethod]
		[ExpectedException(typeof(B2Exception), "Unauthorized error when operating on Buckets. Are you sure the key you are using has access? ")]
		public async Task GetBucketListTest() {
			// Key that is restricted to a specific bucket name above.
			var client = new B2Client(B2Client.Authorize(new B2Options() {
				KeyId = restrictedApplicationKeyId,
				ApplicationKey = restrictedApplicationKey
			}));
			BucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "").Substring(0, 6)}";

			var bucket = await client.Buckets.Create(BucketName, BucketTypes.allPrivate);
		}

		[TestMethod]
		[ExpectedException(typeof(AuthorizationException), "Either KeyId or ApplicationKey were not specified.")]
		public void BadInitialization() {
			// Missing AccountId
			var client = new B2Client(B2Client.Authorize(new B2Options() {
				KeyId = applicationKeyId,
				ApplicationKey = ""
			}));
		}
	}
}
