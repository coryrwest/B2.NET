using System;
using System.Net.Http;
using B2Net.Http.RequestGenerators;
using B2Net.Models;

namespace B2Net.Http {
	public static class FileDeleteRequestGenerator {
		private static class Endpoints {
			public const string Delete = "b2_delete_file_version";
		}

		public static HttpRequestMessage Authorize(B2Options options) {
			var uri = new Uri(Constants.ApiBaseUrl + "/" + Constants.Version + "/" + Endpoints.Delete);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Get,
				RequestUri = uri
			};

			request.Headers.Add("Authorization", Utilities.CreateAuthorizationHeader(options.AccountId, options.ApplicationKey));

			return request;
		}
	}
}
