using System;
using System.Net.Http;
using B2Net.Models;

namespace B2Net.Http {
	public static class FileDownloadRequestGenerators {
		private static class Endpoints {
			public const string DownloadById = "b2_download_file_by_id";
			public const string DownloadByName = "file";
		}

		public static HttpRequestMessage DownloadById(B2Options options, string fileId) {
			var uri = new Uri(options.DownloadUrl + "/b2api/" + Constants.Version + "/" + Endpoints.DownloadById);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent("{\"fileId\":\"" + fileId + "\"}")
			};

			request.Headers.Add("Authorization", options.AuthorizationToken);

			return request;
		}

		public static HttpRequestMessage DownloadByName(B2Options options, string bucketName, string fileName) {
			var uri = new Uri(options.DownloadUrl + "/" + Endpoints.DownloadByName + "/" + bucketName + "/" + fileName.b2UrlEncode());
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Get,
				RequestUri = uri
			};

			request.Headers.Add("Authorization", options.AuthorizationToken);

			return request;
		}
	}
}
