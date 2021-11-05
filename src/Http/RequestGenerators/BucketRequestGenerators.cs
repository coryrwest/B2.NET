﻿using B2Net.Http.RequestGenerators;
using B2Net.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace B2Net.Http {
	public static class BucketRequestGenerators {
		private static class Endpoints {
			public const string List = "b2_list_buckets";
			public const string Create = "b2_create_bucket";
			public const string Delete = "b2_delete_bucket";
			public const string Update = "b2_update_bucket";
		}

		public static HttpRequestMessage GetBucketList(B2Options options) {
			var json = JsonConvert.SerializeObject(new { accountId = options.AccountId });
			return BaseRequestGenerator.PostRequest(Endpoints.List, json, options);
		}

		public static HttpRequestMessage DeleteBucket(B2Options options, string bucketId) {
			var json = JsonConvert.SerializeObject(new { accountId = options.AccountId, bucketId });
			return BaseRequestGenerator.PostRequest(Endpoints.Delete, json, options);
		}

		/// <summary>
		/// Create a bucket. Defaults to allPrivate.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="bucketName"></param>
		/// <param name="bucketType"></param>
		/// <returns></returns>
		public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, string bucketType = "allPrivate") {
			var allowed = new Regex("^[a-zA-Z0-9-]+$");
			if (bucketName.Length < 6 || bucketName.Length > 50 || !allowed.IsMatch(bucketName) || bucketName.StartsWith("b2-")) {
				throw new Exception(@"The bucket name specified does not match the requirements. 
                            Bucket Name can consist of upper-case letters, lower-case letters, numbers, and "" - "", 
                            must be at least 6 characters long, and can be at most 50 characters long");
			}

			var json = JsonConvert.SerializeObject(new { accountId = options.AccountId, bucketName, bucketType });
			return BaseRequestGenerator.PostRequest(Endpoints.Create, json, options);
		}

		/// <summary>
		/// Create a bucket. Defaults to allPrivate.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="bucketName"></param>
		/// <param name="bucketType"></param>
		/// <returns></returns>
		public static HttpRequestMessage CreateBucket(B2Options options, string bucketName, B2BucketOptions bucketOptions) {
			// Check lifecycle rules
			var hasLifecycleRules = bucketOptions.LifecycleRules != null && bucketOptions.LifecycleRules.Count > 0;
			if (hasLifecycleRules) {
				foreach (var rule in bucketOptions.LifecycleRules) {
					if (rule.DaysFromHidingToDeleting < 1 || rule.DaysFromUploadingToHiding < 1) {
						throw new System.Exception("The smallest number of days you can set in a lifecycle rule is 1.");
					}
					if (rule.DaysFromHidingToDeleting == null && rule.DaysFromUploadingToHiding == null) {
						throw new System.Exception("You must set either DaysFromHidingToDeleting or DaysFromUploadingToHiding. Both cannot be null.");
					}
				}
			}

			var allowed = new Regex("^[a-zA-Z0-9-]+$");
			if (bucketName.Length < 6 || bucketName.Length > 50 || !allowed.IsMatch(bucketName) || bucketName.StartsWith("b2-")) {
				throw new Exception(@"The bucket name specified does not match the requirements. 
                            Bucket Name can consist of upper-case letters, lower-case letters, numbers, and "" - "", 
                            must be at least 6 characters long, and can be at most 50 characters long");
			}

			var body = new B2BucketCreateModel() {
				accountId = options.AccountId,
				bucketName = bucketName,
				bucketType = bucketOptions.BucketType.ToString(),
				fileLockEnabled =  bucketOptions.FileLockEnabled
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

			// Has cors rules
			if (bucketOptions.CORSRules != null && bucketOptions.CORSRules.Count > 0) {
				body.corsRules = bucketOptions.CORSRules;
			}

			var json = JsonSerialize(body);
			return BaseRequestGenerator.PostRequest(Endpoints.Create, json, options);
		}

		/// <summary>
		/// Used to modify the bucket type of the provided bucket.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, string bucketType) {
			var json = JsonConvert.SerializeObject(new { accountId = options.AccountId, bucketId, bucketType });
			return BaseRequestGenerator.PostRequest(Endpoints.Update, json, options);
		}

		/// <summary>
		/// Used to modify the bucket type of the provided bucket.
		/// </summary>
		/// <param name="revisionNumber">(optional) When set, the update will only happen if the revision number stored in the B2 service matches the one passed in. This can be used to avoid having simultaneous updates make conflicting changes. </param>
		/// <returns></returns>
		public static HttpRequestMessage UpdateBucket(B2Options options, string bucketId, B2BucketOptions bucketOptions, int? revisionNumber = null) {
			// Check lifecycle rules
			var hasLifecycleRules = bucketOptions.LifecycleRules != null && bucketOptions.LifecycleRules.Count > 0;
			if (hasLifecycleRules) {
				foreach (var rule in bucketOptions.LifecycleRules) {
					if (rule.DaysFromHidingToDeleting < 1 || rule.DaysFromUploadingToHiding < 1) {
						throw new System.Exception("The smallest number of days you can set in a lifecycle rule is 1.");
					}
					if (rule.DaysFromHidingToDeleting == null && rule.DaysFromUploadingToHiding == null) {
						throw new System.Exception("You must set either DaysFromHidingToDeleting or DaysFromUploadingToHiding. Both cannot be null.");
					}
				}
			}

			var body = new B2BucketUpdateModel() {
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

			// Has cors rules
			if (bucketOptions.CORSRules != null && bucketOptions.CORSRules.Count > 0) {
				if (bucketOptions.CORSRules.Any(x => x.AllowedOperations == null || x.AllowedOperations.Length == 0)) {
					throw new System.Exception("You must set allowedOperations on the bucket CORS rules.");
				}
				if (bucketOptions.CORSRules.Any(x => x.AllowedOrigins == null || x.AllowedOrigins.Length == 0)) {
					throw new System.Exception("You must set allowedOrigins on the bucket CORS rules.");
				}
				if (bucketOptions.CORSRules.Any(x => string.IsNullOrEmpty(x.CorsRuleName))) {
					throw new System.Exception("You must set corsRuleName on the bucket CORS rules.");
				}
				body.corsRules = bucketOptions.CORSRules;
			}

			if (revisionNumber.HasValue) {
				body.ifRevisionIs = revisionNumber.Value;
			}

			if (bucketOptions.DefaultRetention != null) {
				body.defaultRetention = bucketOptions.DefaultRetention;
			}

			var json = JsonSerialize(body);
			return BaseRequestGenerator.PostRequest(Endpoints.Update, json, options);
		}

		private static string JsonSerialize(object data) {
			return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings() {
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			});
		}
	}

	internal class B2BucketCreateModel {
		public string accountId { get; set; }
		public string bucketName { get; set; }
		public string bucketType { get; set; }
		public Dictionary<string, string> bucketInfo { get; set; }
		public List<B2BucketLifecycleRule> lifecycleRules { get; set; }
		public List<B2CORSRule> corsRules { get; set; }
		public bool fileLockEnabled { get; set; }
	}

	internal class B2BucketUpdateModel {
		public string accountId { get; set; }
		public string bucketId { get; set; }
		public string bucketType { get; set; }
		public Dictionary<string, string> bucketInfo { get; set; }
		public List<B2BucketLifecycleRule> lifecycleRules { get; set; }
		public List<B2CORSRule> corsRules { get; set; }
		public int? ifRevisionIs { get; set; }
		public B2DefaultRetention defaultRetention { get; set; }
	}
}
