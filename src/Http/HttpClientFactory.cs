using System;
using System.Net.Http;
using System.Net.Http.Headers;
using B2Net.Models;

namespace B2Net.Http {
	public static class HttpClientFactory {
		private static HttpClient _client;

		public static HttpClient CreateTestHttpClient(int timeout) {
			var client = _client;
			if (client == null) {
				var handler = new HttpClientHandler() { AllowAutoRedirect = true };

				client = new HttpClient(handler);

				client.Timeout = TimeSpan.FromSeconds(timeout);

				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				_client = client;
			}

			return client;
		}

		public static HttpClient StaticHttpClient(this B2Options options) {
			return CreateTestHttpClient(options.RequestTimeout);
		}
	}
}
