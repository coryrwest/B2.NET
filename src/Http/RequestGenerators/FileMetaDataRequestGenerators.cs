using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using B2Net.Http.RequestGenerators;
using B2Net.Models;

namespace B2Net.Http {
	public static class FileMetaDataRequestGenerators {
		private static class Endpoints {
			public const string List = "b2_list_file_names";
			public const string Versions = "b2_list_file_versions";
			public const string Hide = "b2_hide_file";
			public const string Info = "b2_get_file_info";
		}

		public static HttpRequestMessage GetList(B2Options options, string bucketId, string startFileName = "", int maxFileCount = 100) {
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(startFileName)) {
				body += ", \"startFileName\":\"" + startFileName + "\"";
			}
			body += "}";
            return BaseRequestGenerator.PostRequest(Endpoints.List, body, options);
		}

		public static HttpRequestMessage ListVersions(B2Options options, string bucketId, string startFileName = "", string startFileId = "", int maxFileCount = 100) {
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(startFileName)) {
				body += ", \"startFileName\":\"" + startFileName + "\"";
			}
			if (!string.IsNullOrEmpty(startFileId)) {
				body += ", \"startFileId\":\"" + startFileId + "\"";
			}
			body += "}";
			return BaseRequestGenerator.PostRequest(Endpoints.Versions, body, options);
		}

		public static HttpRequestMessage HideFile(B2Options options, string bucketId, string fileName = "", string fileId = "") {
            var body = "{\"bucketId\":\"" + bucketId + "\"";
            if (!string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(fileId))
            {
                body += ", \"fileName\":\"" + fileName + "\"";
            }
            if (!string.IsNullOrEmpty(fileId))
            {
                body += ", \"fileId\":\"" + fileName + "\"";
            }
            body += "}";
            return BaseRequestGenerator.PostRequest(Endpoints.Hide, body, options);
		}

		public static HttpRequestMessage GetInfo(B2Options options, string fileId) {
			return BaseRequestGenerator.PostRequest(Endpoints.Info, "{\"fileId\":\"" + fileId + "\"}", options);
		}
	}
}
