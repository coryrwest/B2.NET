using B2Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace B2Net.Http.RequestGenerators {
	public class LargeFileRequestGenerators {
		private static class Endpoints {
			public const string Start = "b2_start_large_file";
			public const string GetPartUrl = "b2_get_upload_part_url";
			public const string Upload = "b2_upload_part";
			public const string Finish = "b2_finish_large_file";
			public const string ListParts = "b2_list_parts";
			public const string Cancel = "b2_cancel_large_file";
			public const string IncompleteFiles = "b2_list_unfinished_large_files";
			public const string CopyPart = "b2_copy_part";
		}

		public static HttpRequestMessage Start(B2Options options, string bucketId, string fileName, string contentType, Dictionary<string, string> fileInfo = null) {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + Endpoints.Start);
			var content = "{\"bucketId\":\"" + bucketId + "\",\"fileName\":\"" + fileName +
											"\",\"contentType\":\"" + (string.IsNullOrEmpty(contentType) ? "b2/x-auto" : contentType) + "\"}";
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(content),
			};

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);
			// File Info headers
			if (fileInfo != null && fileInfo.Count > 0) {
				foreach (var info in fileInfo.Take(10)) {
					request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
				}
			}
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			request.Content.Headers.ContentLength = content.Length;

			return request;
		}

		/// <summary>
		/// Upload a file to B2. This method will calculate the SHA1 checksum before sending any data.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static HttpRequestMessage Upload(B2Options options, byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl) {
			if (partNumber < 1 || partNumber > 10000) {
				throw new Exception("Part number must be between 1 and 10,000");
			}

			var uri = new Uri(uploadPartUrl.UploadUrl);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new ByteArrayContent(fileData)
			};

			// Get the file checksum
			string hash = Utilities.GetSHA1Hash(fileData);

			// Add headers
			request.Headers.TryAddWithoutValidation("Authorization", uploadPartUrl.AuthorizationToken);
			request.Headers.Add("X-Bz-Part-Number", partNumber.ToString());
			request.Headers.Add("X-Bz-Content-Sha1", hash);
			request.Content.Headers.ContentLength = fileData.Length;

			return request;
		}

		public static HttpRequestMessage GetUploadPartUrl(B2Options options, string fileId) {
			return BaseRequestGenerator.PostRequest(Endpoints.GetPartUrl, JsonConvert.SerializeObject(new { fileId }), options);
		}

		public static HttpRequestMessage Finish(B2Options options, string fileId, string[] partSHA1Array) {
			var content = JsonConvert.SerializeObject(new { fileId, partSha1Array = partSHA1Array });
			var request = BaseRequestGenerator.PostRequestJson(Endpoints.Finish, content, options);
			return request;
		}

		public static HttpRequestMessage ListParts(B2Options options, string fileId, int startPartNumber, int maxPartCount) {
			if (startPartNumber < 1 || startPartNumber > 10000) {
				throw new Exception("Start part number must be between 1 and 10,000");
			}

			var content = JsonConvert.SerializeObject(new { fileId, startPartNumber, maxPartCount });
			var request = BaseRequestGenerator.PostRequestJson(Endpoints.ListParts, content, options);
			return request;
		}

		public static HttpRequestMessage Cancel(B2Options options, string fileId) {
			var content = JsonConvert.SerializeObject(new { fileId });
			var request = BaseRequestGenerator.PostRequestJson(Endpoints.Cancel, content, options);
			return request;
		}

		public static HttpRequestMessage IncompleteFiles(B2Options options, string bucketId, string startFileId = "", string maxFileCount = "") {
			var body = "{\"bucketId\":\"" + bucketId + "\"";
			if (!string.IsNullOrEmpty(startFileId)) {
				body += ", \"startFileId\":" + JsonConvert.ToString(startFileId);
			}
			if (!string.IsNullOrEmpty(maxFileCount)) {
				body += ", \"maxFileCount\":" + JsonConvert.ToString(maxFileCount);
			}
			body += "}";
			var request = BaseRequestGenerator.PostRequestJson(Endpoints.IncompleteFiles, body, options);
			return request;
		}

		public static HttpRequestMessage CopyPart(B2Options options, string sourceFileId, string destinationLargeFileId, int destinationPartNumber, string range = "") {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + Endpoints.CopyPart);
			var payload = new Dictionary<string,string>() {
				{ "sourceFileId", sourceFileId },
				{ "largeFileId", destinationLargeFileId },
				{ "partNumber", destinationPartNumber.ToString() },
			};
			if (!string.IsNullOrWhiteSpace(range)) {
				payload.Add("range", range);
			}
			var content = JsonConvert.SerializeObject(payload);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(content),
			};

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			request.Content.Headers.ContentLength = content.Length;

			return request;
		}
	}
}
