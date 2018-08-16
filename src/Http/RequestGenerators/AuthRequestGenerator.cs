using System;
using System.Net.Http;
using B2Net.Models;

namespace B2Net.Http {
	public static class AuthRequestGenerator {
		private static class Endpoints {
			public const string Auth = "b2_authorize_account";
		}

		public static HttpRequestMessage Authorize(B2Options options) {
			var uri = new Uri(Constants.ApiBaseUrl + "/" + Constants.Version + "/" + Endpoints.Auth);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Get,
				RequestUri = uri
			};

      var accountId = !string.IsNullOrWhiteSpace(options.ApplicationKeyId)
        ? options.ApplicationKeyId
        : options.AccountId;

			request.Headers.Add("Authorization", Utilities.CreateAuthorizationHeader(accountId, options.ApplicationKey));

			return request;
		}
	}
}
