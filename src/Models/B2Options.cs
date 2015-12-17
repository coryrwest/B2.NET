namespace B2Net.Models {
	public class B2Options {
		public string AccountId { get; set; }
		public string ApplicationKey { get; set; }
		public string BucketId { get; set; }
		/// <summary>
		/// Setting this to true will use this bucket by default for all
		/// api calls made from this client. Useful if your app will
		/// only ever use one bucket. Default: false.
		/// </summary>
		public bool PersistBucket { get; set; }
		
		// State
		public string ApiUrl { get; set; }
		public string DownloadUrl { get; set; }
		public string AuthorizationToken { get; set; }
		public string UploadAuthorizationToken { get; set; }

		public B2Options() {
			PersistBucket = false;
		}

		public void SetState(B2AuthResponse response) {
			if (response.accountId == AccountId) {
				ApiUrl = response.apiUrl;
				DownloadUrl = response.downloadUrl;
				AuthorizationToken = response.authorizationToken;
			}
		}
	}
}
