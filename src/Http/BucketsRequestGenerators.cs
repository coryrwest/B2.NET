using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using B2Net.Models;

namespace B2Net.Http {
	public static class BucketsRequestGenerators {
		public static HttpRequestMessage GetBucketList(B2Options options) {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/b2_list_buckets");
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent("{\"accountId\":\"" + options.AccountId + "\"}")
			};

			request.Headers.Add("Authorization", options.AuthorizationToken);

			return request;
		} 
	}
}
