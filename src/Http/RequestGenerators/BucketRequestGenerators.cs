using System.Net.Http;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

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
        public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, string bucketType = "allPrivate")
        {
            // TODO: Handle naming conventions, check name for invalid characters.
            var body = "{\"accountId\":\"" + options.AccountId + "\", \"bucketName\":\"" + bucketName +
                        "\", \"bucketType\":\"" + bucketType + "\"}";
            return BaseRequestGenerator.PostRequest(Endpoints.Create, body, options);
        }

        /// <summary>
        /// Create a bucket. Defaults to allPrivate.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="bucketName"></param>
        /// <param name="bucketType"></param>
        /// <returns></returns>
        public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, B2BucketOptions bucketOptions)
        {
            // Check lifecycle rules
            var hasLifecycleRules = bucketOptions.LifecycleRules != null && bucketOptions.LifecycleRules.Count > 0;
            if (hasLifecycleRules) {
                foreach (var rule in bucketOptions.LifecycleRules) {
                    if (rule.DaysFromHidingToDeleting < 1 || rule.DaysFromUploadingToHiding < 1) {
                        throw new System.Exception("The smallest number of days you can set in a lifecycle rule is 1.");
                    }
                }
            }

            // TODO: Handle naming conventions, check name for invalid characters.
            var body = new B2BucketCreateModel() {
                accountId = options.AccountId,
                bucketName = bucketName,
                bucketType = bucketOptions.BucketType.ToString()
            };

            // Add optional options
            if (bucketOptions.CacheControl != 0) {
                body.bucketInfo = new Dictionary<string, string>() {
                    { "Cache-Control", "max-age=" + bucketOptions.CacheControl }
                };
            }
            if(hasLifecycleRules) {
                body.lifecycleRules = bucketOptions.LifecycleRules;
            }

            var json = JsonSerialize(body);
            return BaseRequestGenerator.PostRequest(Endpoints.Create, json, options);
        }

        /// <summary>
        /// Used to modify the bucket type of the provided bucket.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, string bucketType)
        {
            var body = "{\"accountId\":\"" + options.AccountId + "\", \"bucketId\":\"" + bucketId + "\", \"bucketType\":\"" +
                        bucketType + "\"}";
            return BaseRequestGenerator.PostRequest(Endpoints.Update, body, options);
        }

        /// <summary>
        /// Used to modify the bucket type of the provided bucket.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, B2BucketOptions bucketOptions)
        {
            // Check lifecycle rules
            var hasLifecycleRules = bucketOptions.LifecycleRules != null && bucketOptions.LifecycleRules.Count > 0;
            if (hasLifecycleRules) {
                foreach (var rule in bucketOptions.LifecycleRules) {
                    if (rule.DaysFromHidingToDeleting < 1 || rule.DaysFromUploadingToHiding < 1) {
                        throw new System.Exception("The smallest number of days you can set in a lifecycle rule is 1.");
                    }
                }
            }

            var body = new B2BucketUpdateModel()
            {
                accountId = options.AccountId,
                bucketId = bucketId,
                bucketType = bucketOptions.BucketType.ToString()
            };

            // Add optional options
            if (bucketOptions.CacheControl != 0) {
                body.bucketInfo = new Dictionary<string, string>() {
                    { "Cache-Control", "max-age=" + bucketOptions.CacheControl }
                };
            }
            if (hasLifecycleRules) {
                body.lifecycleRules = bucketOptions.LifecycleRules;
            }

            var json = JsonSerialize(body);
            return BaseRequestGenerator.PostRequest(Endpoints.Update, json, options);
        }

        private static string JsonSerialize(object data){
            return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }

    internal class B2BucketCreateModel
    {
        public string accountId { get; set; }
        public string bucketName { get; set; }
        public string bucketType { get; set; }
        public Dictionary<string, string> bucketInfo { get; set; }
        public List<B2BucketLifecycleRule> lifecycleRules { get; set; }
    }

    internal class B2BucketUpdateModel
    {
        public string accountId { get; set; }
        public string bucketId { get; set; }
        public string bucketType { get; set; }
        public Dictionary<string, string> bucketInfo { get; set; }
        public List<B2BucketLifecycleRule> lifecycleRules { get; set; }
    }
}
