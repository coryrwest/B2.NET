using B2Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace B2Net.Http {
	public static class FileCopyRequestGenerators {
		private static class Endpoints {
			public const string Copy = "b2_copy_file";
		}

		public static HttpRequestMessage Copy(B2Options options, string sourceFileId, string fileName, B2MetadataDirective metadataDirective,
			string contentType = "", Dictionary<string, string> fileInfo = null, string range = "") {
			var uri = new Uri(options.DownloadUrl + "/b2api/" + Constants.Version + "/" + Endpoints.Copy);

			var payload = new Dictionary<string, string>() {
				{"sourceFileId", sourceFileId},
				{"fileName", fileName},
				{"metadataDirective", metadataDirective.ToString()},
			};
			if (!string.IsNullOrWhiteSpace(range)) {
				payload.Add("range", range);
			}
			if (metadataDirective == B2MetadataDirective.REPLACE) {
				payload.Add("contentType", contentType);
			}
			var json = JsonConvert.SerializeObject(payload);

			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(json)
			};
			// File Info headers
			if (metadataDirective == B2MetadataDirective.REPLACE && fileInfo != null && fileInfo.Count > 0) {
				foreach (var info in fileInfo.Take(10)) {
					request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
				}
			}

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);
			
			return request;
		}
	}
}
