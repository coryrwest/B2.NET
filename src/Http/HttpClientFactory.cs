using System.Net.Http;
using System.Net.Http.Headers;

namespace B2Net.Http {
	public static class HttpClientFactory {
		public static HttpClient CreateHttpClient() {
			var handler = new HttpClientHandler() {AllowAutoRedirect = true};

			var httpClient = new HttpClient(handler);

			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			return httpClient;
		}
	}
}
