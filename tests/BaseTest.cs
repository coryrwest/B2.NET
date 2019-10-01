using B2Net.Models;

namespace B2Net.Tests {
	public class BaseTest {
		public B2Options Options { get; set; }

		// TODO Change these to valid keys to run tests
		protected string applicationKey = "K001OjBnEmjtx7AS5UYsy+Ii2IFInrM";
		protected string applicationKeyId = "00151189a8b4c7a0000000008";

		protected string restrictedApplicationKey = "K001kwrF6/Tz//Gx1oyrqNyIjJtgVUA";
		protected string restrictedApplicationKeyId = "00151189a8b4c7a0000000009";

		public BaseTest() {
			Options = new B2Options() {
				AccountId = TestConstants.AccountId,
				ApplicationKey = TestConstants.ApplicationKey,
				BucketId = TestConstants.BucketId
			};
		}
	}
}
