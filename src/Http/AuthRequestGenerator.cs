using System;
using System.Net.Http;
using B2Net.Models;

namespace B2Net.Http {
	public static class AuthRequestGenerator {
		public static HttpRequestMessage Authorize(B2Options options) {
			var uri = new Uri(Constants.ApiBaseUrl + "/" + Constants.Version + "/" + Constants.AuthorizeEndpoint);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Get,
				RequestUri = uri
			};

			request.Headers.Add("Authorization", Utilities.CreateAuthorizationHeader(options.AccountId, options.ApplicationKey));

			return request;
		}
	}
}
