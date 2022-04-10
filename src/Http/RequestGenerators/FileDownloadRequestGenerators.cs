using B2Net.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace B2Net.Http {
	public static class FileDownloadRequestGenerators {
		private static class Endpoints {
			public const string DownloadById = "b2_download_file_by_id";
			public const string GetDownloadAuthorization = "b2_get_download_authorization";
			public const string DownloadByName = "file";
		}

		public static HttpRequestMessage DownloadById(B2Options options, string fileId, string byteRange = "") {
			var uri = new Uri(options.DownloadUrl + "/b2api/" + Constants.Version + "/" + Endpoints.DownloadById);

			var json = Utilities.Serialize(new { fileId });
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(json)
			};

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

			// Add byte range header if we have it
			if (!string.IsNullOrEmpty(byteRange)) {
				request.Headers.Add("Range", $"bytes={byteRange}");
			}

			return request;
		}

		public static HttpRequestMessage DownloadByName(B2Options options, string bucketName, string fileName,
			string byteRange = "") {
			var uri = new Uri(options.DownloadUrl + "/" + Endpoints.DownloadByName + "/" + bucketName + "/" +
			                  fileName.b2UrlEncode());
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Get,
				RequestUri = uri
			};

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

			// Add byte range header if we have it
			if (!string.IsNullOrEmpty(byteRange)) {
				request.Headers.Add("Range", $"bytes={byteRange}");
			}

			return request;
		}

		public static HttpRequestMessage GetDownloadAuthorization(B2Options options, string fileNamePrefix,
			int validDurationInSeconds, string bucketId, string b2ContentDisposition = "") {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" +
			                  Endpoints.GetDownloadAuthorization);


			var bodyData = new Dictionary<string, object>() {
				{ "bucketId", bucketId },
				{ "fileNamePrefix", fileNamePrefix },
				{ "validDurationInSeconds", validDurationInSeconds },
				{ "bucketId", bucketId }
			};
			if (!string.IsNullOrEmpty(b2ContentDisposition)) {
				bodyData["b2ContentDisposition"] = b2ContentDisposition;
			}

			var body = Utilities.Serialize(bodyData);

			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(body)
			};

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

			return request;
		}
	}
}
