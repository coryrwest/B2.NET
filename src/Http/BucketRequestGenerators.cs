using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using B2Net.Models;

namespace B2Net.Http {
	public static class BucketRequestGenerators {
		private static class Endpoints {
			public const string List = "b2_list_buckets";
			public const string Create = "b2_create_bucket";
			public const string Delete = "b2_delete_bucket";
			public const string Update = "b2_update_bucket";
		}

		private static HttpRequestMessage BucketRequest(string endpoint, string body, B2Options options) {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + endpoint);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(body)
			};

			request.Headers.Add("Authorization", options.AuthorizationToken);

			return request;
		}

		public static HttpRequestMessage GetBucketList(B2Options options) {
			return BucketRequest(Endpoints.List, "{\"accountId\":\"" + options.AccountId + "\"}", options);
		}

		public static HttpRequestMessage DeleteBucket(B2Options options, string bucketId) {
			return BucketRequest(Endpoints.Delete, "{\"accountId\":\"" + options.AccountId + "\", \"bucketId\":\"" + bucketId + "\"}", options);
		}

		/// <summary>
		/// Create a bucket. Defaults to allPrivate.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="bucketName"></param>
		/// <param name="bucketType"></param>
		/// <returns></returns>
		public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, string bucketType = "allPrivate") {
			// TODO: Handle naming conventions, check name for invalid characters.
			var body = "{\"accountId\":\"" + options.AccountId + "\", \"bucketName\":\"" + bucketName +
						"\", \"bucketType\":\"" + bucketType + "\"}";
			return BucketRequest(Endpoints.Create, body, options);
		}

		/// <summary>
		/// Used to modify the bucket type of the provided bucket.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, string bucketType) {
			var body = "{\"accountId\":\"" + options.AccountId + "\", \"bucketId\":\"" + bucketId + "\", \"bucketType\":\"" +
						bucketType + "\"}";
			return BucketRequest(Endpoints.Update, body, options);
		} 
	}
}
