using B2Net.Http.RequestGenerators;
using B2Net.Models;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;

namespace B2Net.Http {
	public static class FileMetaDataRequestGenerators {
		private static class Endpoints {
			public const string List = "b2_list_file_names";
			public const string Versions = "b2_list_file_versions";
			public const string Hide = "b2_hide_file";
			public const string Info = "b2_get_file_info";
			public const string UpdateFileRetention = "b2_update_file_retention";
		}

		public static HttpRequestMessage GetList(B2Options options, string bucketId, string startFileName = "", int? maxFileCount = null, string prefix = "", string delimiter = "") {
			var requestObj = new {
				bucketId
			};
			
			var properties = new Dictionary<string, object> {
				{ "bucketId", bucketId }
			};
			
			if (!string.IsNullOrEmpty(startFileName)) {
				properties.Add("startFileName", startFileName);
			}
			if (maxFileCount.HasValue) {
				properties.Add("maxFileCount", maxFileCount.Value);
			}
			if (!string.IsNullOrEmpty(prefix)) {
				properties.Add("prefix", prefix);
			}
			if (!string.IsNullOrEmpty(delimiter)) {
				properties.Add("delimiter", delimiter);
			}
			
			var json = JsonSerializer.Serialize(properties);
			return BaseRequestGenerator.PostRequest(Endpoints.List, json, options);
		}

		public static HttpRequestMessage ListVersions(B2Options options, string bucketId, string startFileName = "", string startFileId = "", int? maxFileCount = null, string prefix = "", string delimiter = "") {
			var properties = new Dictionary<string, object> {
				{ "bucketId", bucketId }
			};
			
			if (!string.IsNullOrEmpty(startFileName)) {
				properties.Add("startFileName", startFileName);
			}
			if (!string.IsNullOrEmpty(startFileId)) {
				properties.Add("startFileId", startFileId);
			}
			if (maxFileCount.HasValue) {
				properties.Add("maxFileCount", maxFileCount.Value);
			}
			if (!string.IsNullOrEmpty(prefix)) {
				properties.Add("prefix", prefix);
			}
			if (!string.IsNullOrEmpty(delimiter)) {
				properties.Add("delimiter", delimiter);
			}
			
			var json = JsonSerializer.Serialize(properties);
			return BaseRequestGenerator.PostRequest(Endpoints.Versions, json, options);
		}

		public static HttpRequestMessage HideFile(B2Options options, string bucketId, string fileName = "", string fileId = "") {
			var properties = new Dictionary<string, object> {
				{ "bucketId", bucketId }
			};
			
			if (!string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(fileId)) {
				properties.Add("fileName", fileName);
			}
			if (!string.IsNullOrEmpty(fileId)) {
				properties.Add("fileId", fileId);
			}
			
			var json = JsonSerializer.Serialize(properties);
			return BaseRequestGenerator.PostRequest(Endpoints.Hide, json, options);
		}

		public static HttpRequestMessage GetInfo(B2Options options, string fileId) {
			var json = JsonSerializer.Serialize(new { fileId });
			return BaseRequestGenerator.PostRequest(Endpoints.Info, json, options);
		}

		public static HttpRequestMessage UpdateFileRetention(B2Options options, string fileName, string fileId, B2DefaultRetention fileRetention, bool bypassGovernance = false) {
			var json = JsonSerializer.Serialize(new {
				fileName,
				fileId,
				fileRetention,
				bypassGovernance
			});
			return BaseRequestGenerator.PostRequest(Endpoints.UpdateFileRetention, json, options);
		}
	}
}
