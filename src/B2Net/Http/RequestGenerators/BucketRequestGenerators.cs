using System.Net.Http;
using B2Net.Http.RequestGenerators;
using B2Net.Models;

namespace B2Net.Http {
	public static class BucketRequestGenerators {
		private static class Endpoints {
			public const string List = "b2_list_buckets";
			public const string Create = "b2_create_bucket";
			public const string Delete = "b2_delete_bucket";
			public const string Update = "b2_update_bucket";
		}

		public static HttpRequestMessage GetBucketList(B2Options options) {
			return BaseRequestGenerator.PostRequest(Endpoints.List, "{\"accountId\":\"" + options.AccountId + "\"}", options);
		}

		public static HttpRequestMessage DeleteBucket(B2Options options, string bucketId) {
			return BaseRequestGenerator.PostRequest(Endpoints.Delete, "{\"accountId\":\"" + options.AccountId + "\", \"bucketId\":\"" + bucketId + "\"}", options);
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
			return BaseRequestGenerator.PostRequest(Endpoints.Create, body, options);
		}

		/// <summary>
		/// Used to modify the bucket type of the provided bucket.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, string bucketType) {
			var body = "{\"accountId\":\"" + options.AccountId + "\", \"bucketId\":\"" + bucketId + "\", \"bucketType\":\"" +
						bucketType + "\"}";
			return BaseRequestGenerator.PostRequest(Endpoints.Update, body, options);
		} 
	}
}
