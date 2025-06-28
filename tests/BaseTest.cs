using B2Net.Models;

namespace B2Net.Tests {
	public class BaseTest {
		public B2Options Options { get; set; }

		// TODO Change these to valid keys to run tests
		protected string restrictedApplicationKey = "K001+Bmwc8a5fn+iHgckUZNd6MHLBVA";
		protected string restrictedApplicationKeyId = "00151189a8b4c7a0000000015";

		public BaseTest() {
			Options = new B2Options() {
				KeyId = TestConstants.KeyId,
				ApplicationKey = TestConstants.ApplicationKey
			};
		}
	}
}
