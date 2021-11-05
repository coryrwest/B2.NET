using B2Net.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;

namespace B2Net.Http {
	/// <summary>
	/// These are internal methods and should not be used by consumers of the library.
	/// </summary>
	public static class FileDownloadRequestGenerators {
		private static class Endpoints {
			public const string DownloadById = "b2_download_file_by_id";
			public const string GetDownloadAuthorization = "b2_get_download_authorization";
			public const string DownloadByName = "file";
		}

		public static HttpRequestMessage DownloadById(B2Options options, string fileId, string byteRange = "") {
			var uri = new Uri(options.DownloadUrl + "/b2api/" + Constants.Version + "/" + Endpoints.DownloadById);

			var json = JsonConvert.SerializeObject(new { fileId });
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

		public static HttpRequestMessage DownloadByName(B2Options options, string bucketName, string fileName, string byteRange = "") {
			var uri = new Uri(options.DownloadUrl + "/" + Endpoints.DownloadByName + "/" + bucketName + "/" + fileName.b2UrlEncode());
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

		public class DownloadAuthorizationContext {
			public string bucketId { get; set; }
			public string fileNamePrefix { get; set; }
			[DefaultValue("")]
			public int validDurationInSeconds { get; set; }
			[DefaultValue("")]
			public string b2ContentDisposition { get; set; }
			[DefaultValue("")]
			public string b2ContentLanguage { get; set; }
			[DefaultValue("")]
			public string b2Expires { get; set; }
			[DefaultValue("")]
			public string b2CacheControl { get; set; }
			[DefaultValue("")]
			public string b2ContentEncoding { get; set; }
			[DefaultValue("")]
			public string b2ContentType { get; set; }
		}

		public static HttpRequestMessage GetDownloadAuthorization(B2Options options, string fileNamePrefix, int validDurationInSeconds, string bucketId
			, string b2ContentDisposition = "", string b2ContentLanguage = "", string b2Expires = "", string b2CacheControl = "", string b2ContentEncoding = "", string b2ContentType = "") {
			var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + Endpoints.GetDownloadAuthorization);

			var body = new DownloadAuthorizationContext {
				bucketId = bucketId, fileNamePrefix = fileNamePrefix, validDurationInSeconds = validDurationInSeconds,
				b2ContentDisposition = b2ContentDisposition,
				b2ContentLanguage = b2ContentLanguage,
				b2Expires = b2Expires,
				b2CacheControl = b2CacheControl,
				b2ContentEncoding = b2ContentEncoding,
				b2ContentType = b2ContentType
			};

			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StringContent(JsonConvert.SerializeObject(body, new JsonSerializerSettings {
					NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore
				}))
			};

			request.Headers.TryAddWithoutValidation("Authorization", options.AuthorizationToken);

			return request;
		}
	}
}
