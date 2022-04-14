using B2Net.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace B2Net.Http.RequestGenerators {
	public static class BaseRequestGenerator {
		public static HttpRequestMessage PostRequest(string endpoint, string body, B2Options options) {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + endpoint);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(body)
			};

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

			return request;
		}

		public static async Task<HttpRequestMessage> PostRequest(this B2BaseRequestFactory requestFactory,
			string endpoint, string body) {
			var (authInfo, startRequest) = await requestFactory.StartRequest();
			startRequest.ToEndpoint(HttpMethod.Post, authInfo.apiUrl, endpoint).WithStringContent(body);

			return startRequest;
		}

		public static async Task<HttpRequestMessage> PostRequestJson(this B2BaseRequestFactory requestFactory,
			string endpoint, string body) {
			var (authInfo, startRequest) = await requestFactory.StartRequest();
			startRequest.ToEndpoint(HttpMethod.Post, authInfo.apiUrl, endpoint).WithJson(body);

			return startRequest;
		}

		public static HttpRequestMessage PostRequestJson(string endpoint, string body, B2Options options) {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + endpoint);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(body)
			};

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			request.Content.Headers.ContentLength = body.Length;

			return request;
		}

		public static HttpRequestMessage Request(string authorizationToken) {
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
			};

			request.Headers.TryAddWithoutValidation("Authorization", authorizationToken);

			return request;
		}

		public static HttpRequestMessage ToEndpoint(this HttpRequestMessage request, HttpMethod method, string apiUrl,
			string endpoint) {
			request.RequestUri = new Uri($"{apiUrl}/b2api/{Constants.Version}/{endpoint}");
			request.Method = method;

			return request;
		}

		public static HttpRequestMessage WithJson(this HttpRequestMessage request, string body) {
			request.Content = new StringContent(body);
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			request.Content.Headers.ContentLength = body.Length;

			return request;
		}

		public static HttpRequestMessage WithStringContent(this HttpRequestMessage request, string body) {
			request.Content = new StringContent(body);

			return request;
		}
	}
}
