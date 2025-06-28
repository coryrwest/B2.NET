using System;

namespace B2Net.Models {
	public class B2Options {
		public string AccountId { get; private set; }
		public string KeyId { get; set; }
		public string ApplicationKey { get; set; }
		public string BucketId { get; set; }
		/// <summary>
		/// Setting this to true will use this bucket by default for all
		/// api calls made from this client. Useful if your app will
		/// only ever use one bucket. Default: false.
		/// </summary>
		public bool PersistBucket { get; set; } = false;

		/// <summary>
		/// Specify true if you want to disable the library automatically updating your Authorization Token.
		/// This is only required if you have a long-lived B2Client instance and want to manage the token yourself.
		/// </summary>
		public bool NoTokenRefresh { get; set; } = false;

		// State
		public string ApiUrl { get; set; }
		public string S3ApiUrl { get; set; }
		public string DownloadUrl { get; set; }
		public string AuthorizationToken { get; set; }
		public B2Capabilities Capabilities { get; private set; }
		/// <summary>
		/// This will only be set after a call to the upload API
		/// </summary>
		public string UploadAuthorizationToken { get; set; }
		public long RecommendedPartSize { get; set; }
		public long AbsoluteMinimumPartSize { get; set; }

		/// <summary>
		/// Deprecated: Will always be the same as RecommendedPartSize
		/// </summary>
		public long MinimumPartSize => RecommendedPartSize;

		public int RequestTimeout { get; set; }

		public bool AuthTokenNotExpired => AuthTokenExpiration > DateTime.UtcNow;

		public DateTime AuthTokenExpiration { get; private set; }

		public B2Options() {
			PersistBucket = false;
			RequestTimeout = 100;
		}

		public void SetState(B2AuthResponse response) {
			ApiUrl = response.apiUrl;
			S3ApiUrl = response.s3ApiUrl;
			DownloadUrl = response.downloadUrl;
			AuthorizationToken = response.authorizationToken;
			RecommendedPartSize = response.recommendedPartSize;
			AbsoluteMinimumPartSize = response.absoluteMinimumPartSize;
			AccountId = response.accountId;
			Capabilities = new B2Capabilities(response.allowed);
			// Set auth token expiration to 22 hours from now. Max is 24, make sure we don't accidentally use an expired token because our timing was off.
			AuthTokenExpiration = DateTime.UtcNow.AddHours(22);
		}
	}
}
