using B2.Net.Tests;
using B2Net.Http;
using B2Net.Models;
using B2NET.Tests;

namespace B2Net.Tests {
	public class BaseTest {
		public B2Config Options { get; set; }

		private B2ConfigMap _configMap;

		public BaseTest() {
			_configMap = ConfigHelper.GetTestB2Config();
			Options = _configMap.Configs["RestrictedTestKey"];
		}

		public B2Client CreateB2ClientWithNormalKey() {
			//return new B2Client(Options.KeyId, Options.ApplicationKey, Options.StaticHttpClient());
			var client = new B2Client(_configMap.Configs["UnitTestKey"].KeyId,
				_configMap.Configs["UnitTestKey"].ApplicationKey, Options.StaticHttpClient());
			client.Initialize().Wait();

			return client;
		}

		public B2Client CreateB2ClientWithRestricted() {
			return new B2Client(_configMap.Configs["RestrictedTestKey"].KeyId,
				_configMap.Configs["RestrictedTestKey"].ApplicationKey, Options.StaticHttpClient());
		}
	}
}
