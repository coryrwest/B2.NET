using System;
using System.Threading.Tasks;
using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace B2Net.Tests {
	[TestClass]
	public class AuthorizeTest : BaseTest {
		private string NonMasterKey = "K001LarMmmWDIveFaZz3yvB4uattO+Q";

		[TestMethod]
		public async Task CanWeAuthorize() {
			var client = new B2Client(Options);

			var result = await client.Authorize();

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void CanWeAuthorizeStatic() {
			var result = B2Client.Authorize(Options);
			Console.WriteLine(JsonSerializer.Serialize(result));
			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void CanWeAuthorizeNonMasterKey() {
			var result = B2Client.Authorize(Options.KeyId, Options.ApplicationKey);
			Console.WriteLine(JsonSerializer.Serialize(result));
			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void DoWeGetCapabilitiesOnApplicationKey() {
			var result = B2Client.Authorize(Options.KeyId, Options.ApplicationKey);

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
			
			Assert.IsNotNull(result.Capabilities);
			Assert.IsNotNull(result.Capabilities.Capabilities);
		}

		[TestMethod]
		public void DoWeGetCapabilitiesOnClientWithApplicationKey() {
			var client = new B2Client(B2Client.Authorize(Options.KeyId, Options.ApplicationKey));
			
			Assert.IsNotNull(client.Capabilities.Capabilities);
		}

		[TestMethod]
		[ExpectedException(typeof(AuthorizationException))]
		public async Task ErrorAuthorizeNonMasterKeyWithMissingKeyID() {
			var result = await B2Client.AuthorizeAsync("", NonMasterKey);
		}

		[TestMethod]
		public void DoWeGetOptionsBack() {
			var result = B2Client.Authorize(Options);

			Assert.AreNotEqual(0, result.AbsoluteMinimumPartSize);
			Assert.AreNotEqual(0, result.MinimumPartSize);
			Assert.AreNotEqual(0, result.RecommendedPartSize);
			Assert.IsFalse(string.IsNullOrEmpty(result.DownloadUrl));
			Assert.IsFalse(string.IsNullOrEmpty(result.ApiUrl));
		}
	}
}
