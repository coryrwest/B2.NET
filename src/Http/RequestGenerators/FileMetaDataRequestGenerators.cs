using B2Net.Http.RequestGenerators;
using B2Net.Models;
using System.Text.Json;
using System.Net.Http;

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
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(startFileName)) {
				body += ", \"startFileName\":" + JsonSerializer.Serialize(startFileName);
			}
			if (maxFileCount.HasValue) {
				body += ", \"maxFileCount\":" + maxFileCount.Value.ToString();
			}
			if (!string.IsNullOrEmpty(prefix)) {
				body += ", \"prefix\":" + JsonSerializer.Serialize(prefix);
			}
			if (!string.IsNullOrEmpty(delimiter)) {
				body += ", \"delimiter\":" + JsonSerializer.Serialize(delimiter);
			}
			body += "}";
			return BaseRequestGenerator.PostRequest(Endpoints.List, body, options);
		}

		public static HttpRequestMessage ListVersions(B2Options options, string bucketId, string startFileName = "", string startFileId = "", int? maxFileCount = null, string prefix = "", string delimiter = "") {
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(startFileName)) {
				body += ", \"startFileName\":" + JsonSerializer.Serialize(startFileName);
			}
			if (!string.IsNullOrEmpty(startFileId)) {
				body += ", \"startFileId\":\"" + startFileId + "\"";
			}
			if (maxFileCount.HasValue) {
				body += ", \"maxFileCount\":" + maxFileCount.Value.ToString();
			}
			if (!string.IsNullOrEmpty(prefix)) {
				body += ", \"prefix\":" + JsonSerializer.Serialize(prefix);
			}
			if (!string.IsNullOrEmpty(delimiter)) {
				body += ", \"delimiter\":" + JsonSerializer.Serialize(delimiter);
			}
			body += "}";
			return BaseRequestGenerator.PostRequest(Endpoints.Versions, body, options);
		}

		public static HttpRequestMessage HideFile(B2Options options, string bucketId, string fileName = "", string fileId = "") {
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(fileId)) {
				body += ", \"fileName\":" + JsonSerializer.Serialize(fileName);
			}
			if (!string.IsNullOrEmpty(fileId)) {
				body += ", \"fileId\":\"" + fileId + "\"";
			}
			body += "}";
			return BaseRequestGenerator.PostRequest(Endpoints.Hide, body, options);
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
