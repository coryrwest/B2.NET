using B2Net;
using B2Net.Models;
using B2Net.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Net.Tests {
	[TestClass]
	public class AuthorizeTest : BaseTest {
		// TODO Change these to valid keys to run tests
		string applicationKey = "K001+GGkBNcbJVj3LD4+e3s5pCUMQ7U";
		string applicationKeyId = "00151189a8b4c7a0000000006";

		[TestMethod]
		public void CanWeAuthorize() {
			var client = new B2Client(Options);

			var result = client.Authorize().Result;

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void CanWeAuthorizeStatic() {
			var result = B2Client.Authorize(Options);

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void CanWeAuthorizeNonMasterKey() {
			var result = B2Client.Authorize(TestConstants.AccountId, applicationKey, applicationKeyId);
			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void DoWeGetCapabilitiesOnApplicationKey() {
			var result = B2Client.Authorize(applicationKeyId, applicationKey);

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
			
			Assert.IsNotNull(result.Capabilities);
			Assert.IsNotNull(result.Capabilities.Capabilities);
		}

		[TestMethod]
		[ExpectedException(typeof(AuthorizationException))]
		public void ErrorAuthorizeNonMasterKeyWithAccountID() {
			var key = "K001LarMmmWDIveFaZz3yvB4uattO+Q";

			var result = B2Client.Authorize(Options.AccountId, key);
		}

		[TestMethod]
		public void DoWeGetOptionsBack() {
			var result = B2Client.Authorize(Options);

			Assert.AreNotEqual("0", result.AbsoluteMinimumPartSize);
			Assert.AreNotEqual("0", result.MinimumPartSize);
			Assert.AreNotEqual("0", result.RecommendedPartSize);
			Assert.IsFalse(string.IsNullOrEmpty(result.DownloadUrl));
			Assert.IsFalse(string.IsNullOrEmpty(result.ApiUrl));
		}
	}
}
