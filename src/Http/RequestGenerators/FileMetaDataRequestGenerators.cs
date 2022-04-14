using System.Collections.Generic;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace B2Net.Http {
	public static class FileMetaDataRequestGenerators {
		private static class Endpoints {
			public const string List = "b2_list_file_names";
			public const string Versions = "b2_list_file_versions";
			public const string Hide = "b2_hide_file";
			public const string Info = "b2_get_file_info";
		}

		public static async Task<HttpRequestMessage> GetList(B2BaseRequestFactory requestFactory, string bucketId,
			string startFileName = "",
			int? maxFileCount = null, string prefix = "", string delimiter = "") {
			var bodyData = new Dictionary<string, object>() {
				{ "bucketId", bucketId },
			};
			if (!string.IsNullOrEmpty(startFileName)) {
				bodyData["startFileName"] = startFileName;
			}

			if (maxFileCount.HasValue) {
				bodyData["maxFileCount"] = maxFileCount.Value;
			}

			if (!string.IsNullOrEmpty(prefix)) {
				bodyData["prefix"] = prefix;
			}

			if (!string.IsNullOrEmpty(delimiter)) {
				bodyData["delimiter"] = delimiter;
			}

			var body = Utilities.Serialize(bodyData);


			return await requestFactory.PostRequest(Endpoints.List, body);
		}

		public static async Task<HttpRequestMessage> ListVersions(B2BaseRequestFactory requestFactory, string bucketId,
			string startFileName = "",
			string startFileId = "", int? maxFileCount = null, string prefix = "", string delimiter = "") {
			var bodyData = new Dictionary<string, object>() {
				{ "bucketId", bucketId },
			};
			if (!string.IsNullOrEmpty(startFileId)) {
				bodyData["startFileId"] = startFileId;
			}

			if (!string.IsNullOrEmpty(startFileName)) {
				bodyData["startFileName"] = startFileName;
			}

			if (maxFileCount.HasValue) {
				bodyData["maxFileCount"] = maxFileCount.Value;
			}

			if (!string.IsNullOrEmpty(prefix)) {
				bodyData["prefix"] = prefix;
			}

			if (!string.IsNullOrEmpty(delimiter)) {
				bodyData["delimiter"] = delimiter;
			}

			var body = Utilities.Serialize(bodyData);

			return await requestFactory.PostRequest(Endpoints.Versions, body);
		}

		public static async Task<HttpRequestMessage> HideFile(B2BaseRequestFactory requestFactory, string bucketId,
			string fileName = "",
			string fileId = "") {
			var bodyData = new Dictionary<string, object>() {
				{ "bucketId", bucketId },
			};
			if (!string.IsNullOrEmpty(fileName)) {
				bodyData["fileName"] = fileName;
			}

			if (!string.IsNullOrEmpty(fileId)) {
				bodyData["fileId"] = fileId;
			}

			var body = Utilities.Serialize(bodyData);

			return await requestFactory.PostRequest(Endpoints.Hide, body);
		}

		public static async Task<HttpRequestMessage> GetInfo(B2BaseRequestFactory requestFactory, string fileId) {
			var json = Utilities.Serialize(new { fileId });

			return await requestFactory.PostRequest(Endpoints.Info, json);
		}
	}
}
