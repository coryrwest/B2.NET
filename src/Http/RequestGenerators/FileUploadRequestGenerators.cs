using System;
using System.Net.Http;
using System.Net.Http.Headers;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace B2Net.Http {
	public static class FileUploadRequestGenerators {
		private static class Endpoints {
			public const string GetUploadUrl = "b2_get_upload_url";
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
		public static HttpRequestMessage Upload(B2Options options, string uploadUrl, byte[] fileData, string fileName, Dictionary<string, string> fileInfo, string contentType = "") {
			var uri = new Uri(uploadUrl);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new ByteArrayContent(fileData)
			};

			// Get the file checksum
			string hash = Utilities.GetSHA1Hash(fileData);

			// Add headers
			request.Headers.TryAddWithoutValidation("Authorization", options.UploadAuthorizationToken);
			request.Headers.Add("X-Bz-File-Name", fileName.b2UrlEncode());
			request.Headers.Add("X-Bz-Content-Sha1", hash);
			// File Info headers
			if (fileInfo != null && fileInfo.Count > 0) {
				foreach (var info in fileInfo.Take(10)) {
					request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
				}
			}

			request.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "b2/x-auto" : contentType);
			request.Content.Headers.ContentLength = fileData.Length;

			return request;
		}

		/// <summary>
		/// Upload a file to B2 using a stream. NOTE: You MUST provide the SHA1 at the end of your stream. This method will NOT do it for you.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static HttpRequestMessage Upload(B2Options options, string uploadUrl, Stream fileDataWithSHA, string fileName, Dictionary<string, string> fileInfo, string contentType = "", bool dontSHA = false) {
			var uri = new Uri(uploadUrl);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StreamContent(fileDataWithSHA)
			};

			// Add headers
			request.Headers.TryAddWithoutValidation("Authorization", options.UploadAuthorizationToken);
			request.Headers.Add("X-Bz-File-Name", fileName.b2UrlEncode());
			// Stream puts the SHA1 at the end of the content
			request.Headers.Add("X-Bz-Content-Sha1", dontSHA ? "do_not_verify" : "hex_digits_at_end");
			// File Info headers
			if (fileInfo != null && fileInfo.Count > 0) {
				foreach (var info in fileInfo.Take(10)) {
					request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
				}
			}

			request.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "b2/x-auto" : contentType);
			// SHA will be in Stream already
			request.Content.Headers.ContentLength = fileDataWithSHA.Length;

			return request;
		}

		/// <summary>
		/// Upload a file to B2 using a stream. NOTE: You MUST provide the SHA1 at the end of your stream. This method will NOT do it for you.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="uploadContext">Used for additional options when uploading to B2. File Locks, Legal Holds, Content Disposition, etc.</param>
		/// <returns></returns>
		public static HttpRequestMessage Upload(B2Options options, string uploadUrl, Stream fileDataWithSHA, B2FileUploadContext uploadContext, bool dontSHA = false) {
			var uri = new Uri(uploadUrl);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StreamContent(fileDataWithSHA)
			};

			// Add headers
			request.Headers.TryAddWithoutValidation("Authorization", options.UploadAuthorizationToken);
			request.Headers.Add("X-Bz-File-Name", uploadContext.FileName.b2UrlEncode());
			// Stream puts the SHA1 at the end of the content
			request.Headers.Add("X-Bz-Content-Sha1", dontSHA || string.IsNullOrEmpty(uploadContext.ContentSHA) ? "do_not_verify" : "hex_digits_at_end");
			// File Info headers
			if (uploadContext.AdditionalFileInfo != null && uploadContext.AdditionalFileInfo.Count > 0) {
				foreach (var info in uploadContext.AdditionalFileInfo.Take(10)) {
					request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
				}
			}

			if (uploadContext.SrcLastModifiedMillis != 0) {
				request.Headers.Add("X-Bz-src_last_modified_millis", uploadContext.SrcLastModifiedMillis.ToString());
			}
			if (!string.IsNullOrWhiteSpace(uploadContext.ContentDisposition)) {
				request.Headers.Add("X-Bz-Info-b2-content-disposition", uploadContext.ContentDisposition);
			}
			if (!string.IsNullOrWhiteSpace(uploadContext.ContentLanguage)) {
				request.Headers.Add("X-Bz-Info-b2-content-language", uploadContext.ContentLanguage);
			}
			if (uploadContext.Expires != null) {
				request.Headers.Add("X-Bz-Info-b2-expires", uploadContext.Expires.Value.ToString("R"));
			}
			if (!string.IsNullOrWhiteSpace(uploadContext.CacheControl)) {
				request.Headers.Add("X-Bz-Info-b2-cache-control", uploadContext.CacheControl);
			}
			if (uploadContext.ContentEncoding != null) {
				request.Headers.Add("X-Bz-Info-b2-content-encoding", uploadContext.ContentEncoding.Value.ToString());
			}
			if (uploadContext.LegalHold) {
				request.Headers.Add("X-Bz-File-Legal-Hold", "on");
			}

			var retentionSet = false;
			if (uploadContext.RetentionMode != null) {
				retentionSet = true;
				request.Headers.Add("X-Bz-File-Retention-Mode", uploadContext.RetentionMode.Value.ToString());
			}
			// If retention is set and there is no timestamp, fail. 
			if (uploadContext.RetainUntilTimestamp == 0 && retentionSet) {
				throw new ArgumentException("IF you specify a RetentionMode, you must also set the RetainUntilTimestamp");
			}

			if (uploadContext.RetainUntilTimestamp != 0) {
				request.Headers.Add("X-Bz-File-Retain-Until-Timestamp", uploadContext.RetainUntilTimestamp.ToString());
			}

			request.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(uploadContext.ContentType) ? "b2/x-auto" : uploadContext.ContentType);
			// SHA will be in Stream already
			request.Content.Headers.ContentLength = fileDataWithSHA.Length;

			return request;
		}

		public static HttpRequestMessage GetUploadUrl(B2Options options, string bucketId) {
			var json = JsonConvert.SerializeObject(new { bucketId });
			return BaseRequestGenerator.PostRequest(Endpoints.GetUploadUrl, json, options);
		}
	}
}
