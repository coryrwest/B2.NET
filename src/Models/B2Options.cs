namespace B2Net.Models {
	public class B2Options {
		public string AccountId { get; set; }
		public string ApplicationKey { get; set; }
		public string BucketId { get; set; }
		
		// State
		public string ApiUrl { get; set; }
		public string DownloadUrl { get; set; }
		public string AuthorizationToken { get; set; }

		public void SetState(B2AuthResponse response) {
			if (response.accountId == AccountId) {
				ApiUrl = response.apiUrl;
				DownloadUrl = response.downloadUrl;
				AuthorizationToken = response.authorizationToken;
			}
		}
	}
}
