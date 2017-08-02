using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2.Net.Tests;
using B2Net.Models;

namespace B2Net.Tests {
	public class BaseTest {
		public B2Options Options { get; set; }

		public BaseTest() {
			Options = new B2Options() {
				AccountId = TestConstants.AccountId,
				ApplicationKey = TestConstants.ApplicationKey,
				BucketId = TestConstants.BucketId
			};
		}
	}
}
