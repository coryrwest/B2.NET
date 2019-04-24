using B2Net.Http.RequestGenerators;
using B2Net.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace B2Net.Http {
	public static class FileMetaDataRequestGenerators {
		private static class Endpoints {
			public const string List = "b2_list_file_names";
			public const string Versions = "b2_list_file_versions";
			public const string Hide = "b2_hide_file";
			public const string Info = "b2_get_file_info";
		}

		public static HttpRequestMessage GetList(B2Options options, string bucketId, string startFileName = "", int? maxFileCount = null, string prefix = "", string delimiter = "") {
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(startFileName)) {
				body += ", \"startFileName\":" + JsonConvert.ToString(startFileName);
			}
			if (maxFileCount.HasValue) {
				body += ", \"maxFileCount\":" + maxFileCount.Value.ToString();
			}
			if (!string.IsNullOrEmpty(prefix)) {
				body += ", \"prefix\":" + JsonConvert.ToString(prefix);
			}
			if (!string.IsNullOrEmpty(delimiter)) {
				body += ", \"delimiter\":" + JsonConvert.ToString(delimiter);
			}
			body += "}";
			return BaseRequestGenerator.PostRequest(Endpoints.List, body, options);
		}

		public static HttpRequestMessage ListVersions(B2Options options, string bucketId, string startFileName = "", string startFileId = "", int? maxFileCount = null, string prefix = "", string delimiter = "") {
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(startFileName)) {
				body += ", \"startFileName\":" + JsonConvert.ToString(startFileName);
			}
			if (!string.IsNullOrEmpty(startFileId)) {
				body += ", \"startFileId\":\"" + startFileId + "\"";
			}
			if (maxFileCount.HasValue) {
				body += ", \"maxFileCount\":" + maxFileCount.Value.ToString();
			}
			if (!string.IsNullOrEmpty(prefix)) {
				body += ", \"prefix\":" + JsonConvert.ToString(prefix);
			}
			if (!string.IsNullOrEmpty(delimiter)) {
				body += ", \"delimiter\":" + JsonConvert.ToString(delimiter);
			}
			body += "}";
			return BaseRequestGenerator.PostRequest(Endpoints.Versions, body, options);
		}

		public static HttpRequestMessage HideFile(B2Options options, string bucketId, string fileName = "", string fileId = "") {
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(fileId)) {
				body += ", \"fileName\":" + JsonConvert.ToString(fileName);
			}
			if (!string.IsNullOrEmpty(fileId)) {
				body += ", \"fileId\":\"" + fileId + "\"";
			}
			body += "}";
			return BaseRequestGenerator.PostRequest(Endpoints.Hide, body, options);
		}

		public static HttpRequestMessage GetInfo(B2Options options, string fileId) {
			var json = JsonConvert.SerializeObject(new { fileId });
			return BaseRequestGenerator.PostRequest(Endpoints.Info, json, options);
		}
	}
}
