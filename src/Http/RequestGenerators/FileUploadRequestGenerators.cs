using System;
using System.Net.Http;
using System.Net.Http.Headers;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using System.Collections.Generic;
using System.Linq;

namespace B2Net.Http {
	public static class FileUploadRequestGenerators {
		private static class Endpoints {
			public const string GetUploadUrl = "b2_get_upload_url";
		}

		public static HttpRequestMessage Upload(B2Options options, string uploadUrl, byte[] fileData, string fileName, Dictionary<string, string> fileInfo) {
			var uri = new Uri(uploadUrl);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new ByteArrayContent(fileData)
			};

			// Get the file checksum
			string hash = Utilities.GetSHA1Hash(fileData);

			// Add headers
			request.Headers.Add("Authorization", options.UploadAuthorizationToken);
			request.Headers.Add("X-Bz-File-Name", fileName);
			request.Headers.Add("X-Bz-Content-Sha1", hash);
            // File Info headers
            if(fileInfo != null && fileInfo.Count > 0)
            {
                foreach (var info in fileInfo.Take(10))
                {
                    request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
                }
            }
            // TODO last modified
            //request.Headers.Add("X-Bz-src_last_modified_millis", hash);

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("b2/x-auto");
			request.Content.Headers.ContentLength = fileData.Length;

			return request;
		}

		public static HttpRequestMessage GetUploadUrl(B2Options options, string bucketId) {
			return BaseRequestGenerator.PostRequest(Endpoints.GetUploadUrl, "{\"bucketId\":\"" + bucketId + "\"}", options);
		}
	}
}
