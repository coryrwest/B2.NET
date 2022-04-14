using System;
using System.Text;

namespace B2Net.Models {
	public class B2Config {
		public B2Config() {
		}

		public B2Config(string keyId, string applicationKey, int requestTimeout) {
			KeyId = keyId;
			ApplicationKey = applicationKey;
			RequestTimeout = requestTimeout;
		}

		public string KeyId { get; init; }
		public string ApplicationKey { get; init; }
		public string KeyName { get; init; }
		public int RequestTimeout { get; init; }

		public string AccountId { get; init; }

		public string BucketId { get; init; }

		/// <summary>
		/// Setting this to true will use this bucket by default for all
		/// api calls made from this client. Useful if your app will
		/// only ever use one bucket. Default: false.
		/// </summary>
		public bool PersistBucket { get; init; }
	}
}
