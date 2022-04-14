using B2Net.Models;
using System;
using System.Net.Http;

namespace B2Net.Http {
	public static class AuthRequestGenerator {
		private static class Endpoints {
			public const string Auth = "b2_authorize_account";
		}

		public static HttpRequestMessage Authorize(B2Options options) {
			var keyId = options.KeyId;
			var applicationKey = options.ApplicationKey;

			return Authorize(keyId, applicationKey);
		}

		public static HttpRequestMessage Authorize(string keyId, string applicationKey) {
			if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(applicationKey)) {
				throw new AuthorizationException("Either KeyId or ApplicationKey were not specified.");
			}

			var uri = new Uri(Constants.ApiBaseUrl + "/" + Constants.Version + "/" + Endpoints.Auth);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Get,
				RequestUri = uri
			};

			request.Headers.Add("Authorization", Utilities.CreateAuthorizationHeader(keyId, applicationKey));

			return request;
		}
	}
}
