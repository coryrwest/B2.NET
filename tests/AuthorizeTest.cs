using System;
using B2Net;
using B2Net.Models;
using B2Net.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2.Net.Tests {
	[TestClass]
	public class AuthorizeTest : BaseTest {
		[TestMethod]
		public void CanWeAuthorize() {
			var client = new B2Client(Options);

			var result = client.Authorize().Result;

			Assert.IsFalse(string.IsNullOrEmpty(result.AuthorizationToken));
		}
	}
}
