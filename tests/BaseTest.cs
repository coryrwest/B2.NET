using B2Net.Models;

namespace B2Net.Tests {
	public class BaseTest {
		public B2Options Options { get; set; }

		// TODO Change these to valid keys to run tests
		protected string applicationKey = "K001rZuquGfCQN8BHH6lfnew+BkQrEc";
		protected string applicationKeyId = "00151189a8b4c7a000000000b";

		protected string restrictedApplicationKey = "K001rZuquGfCQN8BHH6lfnew+BkQrEc";
		protected string restrictedApplicationKeyId = "00151189a8b4c7a000000000b";

		public BaseTest() {
			Options = new B2Options() {
				KeyId = TestConstants.KeyId,
				ApplicationKey = TestConstants.ApplicationKey,
				BucketId = TestConstants.BucketId
			};
		}
	}
}
