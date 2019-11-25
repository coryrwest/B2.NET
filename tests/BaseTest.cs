using B2Net.Models;

namespace B2Net.Tests {
	public class BaseTest {
		public B2Options Options { get; set; }

		// TODO Change these to valid keys to run tests
		protected string applicationKey = "K001yv7nwRFVFYo5Dnc6Wy/iw4b9KmY";
		protected string applicationKeyId = "00151189a8b4c7a000000000c";

		protected string restrictedApplicationKey = "K0019m9qz095omc+WsnREy5mWsxNmtQ";
		protected string restrictedApplicationKeyId = "00151189a8b4c7a000000000d";

		public BaseTest() {
			Options = new B2Options() {
				KeyId = TestConstants.KeyId,
				ApplicationKey = TestConstants.ApplicationKey,
				BucketId = TestConstants.BucketId
			};
		}
	}
}
