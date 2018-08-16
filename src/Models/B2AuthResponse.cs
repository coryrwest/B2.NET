namespace B2Net.Models {
	public class B2AuthResponse {
    public B2AuthAllowedResponse allowed { get; set; }
		public string accountId { get; set; }
		public string apiUrl { get; set; }
		public string authorizationToken { get; set; }
		public string downloadUrl { get; set; }
        public long recommendedPartSize { get; set; }
        public long absoluteMinimumPartSize { get; set; }
        public long minimumPartSize { get; set; }
    }
}
