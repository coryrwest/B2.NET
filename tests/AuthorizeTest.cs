using System;
using System.Diagnostics;
using System.Threading.Tasks;
using B2Net.Tests;
using B2Net;
using B2Net.Models;
using B2Net.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace B2Net.Tests {
	[TestClass]
	public class AuthorizeTest : BaseTest {
		[TestMethod]
		public void CanWeAuthorize() {
			var client = new B2Client(Options);

			var result = client.Authorize().Result;

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void CanWeAuthorizeStatic() {
			var result = B2Client.Authorize(Options);
			Console.WriteLine(JsonConvert.SerializeObject(result));
			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void CanWeAuthorizeNonMasterKey() {
			var result = B2Client.Authorize(Options.KeyId, Options.ApplicationKey);
			Console.WriteLine(JsonConvert.SerializeObject(result));
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
			var key = "K001LarMmmWDIveFaZz3yvB4uattO+Q";

			var result = await B2Client.AuthorizeAsync("", key);
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
