using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using B2Net.Models;

namespace B2Net.Http.RequestGenerators {
	public static class BaseRequestGenerator {
		public static HttpRequestMessage PostRequest(string endpoint, string body, B2Options options) {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + endpoint);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(body)
			};

			request.Headers.Add("Authorization", options.AuthorizationToken);

			return request;
		}
	}
}
