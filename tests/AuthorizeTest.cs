using System;
using System.Diagnostics;
using System.Threading.Tasks;
using B2Net.Tests;
using B2Net;
using B2Net.Http;
using B2Net.Models;
using B2Net.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace B2Net.Tests {
	[TestClass]
	public class AuthorizeTest : BaseTest {
		[TestMethod]
		public void CanWeAuthorize() {
			var client = CreateB2ClientWithNormalKey();

			var result = client.Authorize().Result;

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void CanWeAuthorizeStatic() {
			var result = B2Client.Authorize(Options, Options.StaticHttpClient());
			Console.WriteLine(Utilities.Serialize(result));
			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void CanWeAuthorizeNonMasterKey() {
			var result = B2Client.Authorize(Options, Options.StaticHttpClient());
			Console.WriteLine(Utilities.Serialize(result));
			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}

		[TestMethod]
		public void DoWeGetCapabilitiesOnApplicationKey() {
			var result = B2Client.Authorize(Options, Options.StaticHttpClient());

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));

			Assert.IsNotNull(result.Capabilities);
			Assert.IsNotNull(result.Capabilities.Capabilities);
		}

		[TestMethod]
		public void DoWeGetCapabilitiesOnClientWithApplicationKey() {
			var client = CreateB2ClientWithNormalKey();

			Assert.IsNotNull(client.Capabilities.Capabilities);
		}

		[TestMethod]
		[ExpectedException(typeof(AuthorizationException))]
		public async Task ErrorAuthorizeNonMasterKeyWithMissingKeyID() {
			var key = "K001LarMmmWDIveFaZz3yvB4uattO+Q";

			var result = await B2Client.AuthorizeAsync("", key, Options.StaticHttpClient());
		}

		[TestMethod]
		public void DoWeGetOptionsBack() {
			var result = B2Client.Authorize(Options, Options.StaticHttpClient());

			Assert.AreNotEqual("0", result.AbsoluteMinimumPartSize);
			Assert.AreNotEqual("0", result.MinimumPartSize);
			Assert.AreNotEqual("0", result.RecommendedPartSize);
			Assert.IsFalse(string.IsNullOrEmpty(result.DownloadUrl));
			Assert.IsFalse(string.IsNullOrEmpty(result.ApiUrl));
		}
	}
}
